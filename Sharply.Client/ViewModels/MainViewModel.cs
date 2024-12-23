using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;
using Sharply.Client.Models;
using Sharply.Client.Views;
using Sharply.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sharply.Client.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private readonly ITokenStorageService _tokenStorageService;
    private readonly INavigationService _navigationService;
    private readonly ISignalRService _signalRService;
    private readonly IOverlayService _overlayService;

    #region Constructors

    public MainViewModel(
        IApiService apiService,
        ITokenStorageService tokenStorageService,
        INavigationService navigationService,
        ISignalRService signalRService,
        IOverlayService overlayService)
    {
        _apiService = apiService;
        _tokenStorageService = tokenStorageService;
        _signalRService = signalRService;
        _overlayService = overlayService;
        _navigationService = navigationService;

        InitializeEvents();
        InitializeCommands();

        _navigationService.NavigateTo<LoginViewModel>();
        IsServerSelected = false;
    }

    public MainViewModel() { }

    #endregion

    #region Commands
    public IRelayCommand? AddChannelCommand { get; set; }
    public IRelayCommand? SendMessageCommand { get; set; }
    public IRelayCommand? OpenServerSettingsCommand { get; set; }
    public IRelayCommand? OpenUserSettingsCommands { get; set; }
    public IRelayCommand? LogoutCommand { get; set; }
    #endregion

    #region Properties

    public string Title { get; } = "Sharply";
    private CurrentUser CurrentUser { get; set; }

    [ObservableProperty]
    private ObservableCollection<ServerViewModel> _servers = new();

    [ObservableProperty]
    private ServerViewModel? _selectedServer;

    [ObservableProperty]
    private bool _isServerSelected;

    [ObservableProperty]
    private ObservableCollection<ChannelViewModel> _channels = new();

    [ObservableProperty]
    private ChannelViewModel? _selectedChannel;

    [ObservableProperty]
    private ObservableCollection<MessageViewModel> _messages = new();

    [ObservableProperty]
    private ObservableCollection<UserViewModel> _onlineUsers = new();

    [ObservableProperty]
    private string _newMessage = string.Empty;

    [ObservableProperty]
    private string? _channelDisplayName;

    public object? CurrentView => _navigationService.CurrentView;
    public object? CurrentOverlay => _overlayService.CurrentOverlayView;
    public bool IsOverlayVisible => _overlayService.IsOverlayVisible;

    private List<UserDto> _globalOnlineUsers = new();

    #endregion

    #region Methods

    public void InitializeEvents()
    {
        _overlayService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_overlayService.IsOverlayVisible))
                OnPropertyChanged(nameof(IsOverlayVisible));

            if (e.PropertyName == nameof(_overlayService.CurrentOverlayView))
                OnPropertyChanged(nameof(CurrentOverlay));
        };

        _navigationService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_navigationService.CurrentView))
                OnPropertyChanged(nameof(CurrentView));
        };
    }

    public void InitializeCommands()
    {
        AddChannelCommand = new RelayCommand(AddChannel);
        SendMessageCommand = new RelayCommand(SendMessage);
        OpenServerSettingsCommand = new RelayCommand(OpenServerSettings);
    }

    public async Task LoadInitialData()
    {
        try
        {
            // Get the token, and parse the data from it
            var token = _tokenStorageService.LoadToken();
            if (token == null) throw new Exception("token was null");

            var userData = await _apiService.GetCurrentUserData(token);
            if (userData == null)
                throw new Exception("User data missing.");

            CurrentUser = CurrentUser.FromDto(userData);

            // Retrieve all servers and channels associated with the user
            var servers = await _apiService.GetServersAsync(token);
            Servers = new ObservableCollection<ServerViewModel>(servers);

            var channels = servers.SelectMany(s => s.Channels);
            Channels = new ObservableCollection<ChannelViewModel>(channels);

            await InitializeHubConnections(token);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
    public async Task InitializeHubConnections(string token)
    {
        try
        {
            await _signalRService.ConnectMessageHubAsync(token);
            await _signalRService.ConnectUserHubAsync(token);

            _signalRService.OnMessageReceived(OnMessageReceived);
            _signalRService.OnOnlineUsersUpdated(users => OnOnlineUsersUpdatedAsync(users).Wait());

            await _signalRService.GoOnline(CurrentUser.Id);

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing SignalR HubConnection: {ex.Message}");
        }
    }

    private void AddServer()
    {
    }

    private void AddChannel()
    {
    }

    private async void SendMessage()
    {
        try
        {
            if (SelectedChannel?.Id == null || string.IsNullOrWhiteSpace(NewMessage))
                return;

            await _signalRService.SendMessageAsync(SelectedChannel.Id.Value, CurrentUser.Id, NewMessage);
            NewMessage = string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    partial void OnSelectedServerChanged(ServerViewModel? value)
    {
        if (value == null) return;

        Channels.Clear();
        Channels = new ObservableCollection<ChannelViewModel>(value.Channels);
        if (Channels.Any())
        {
            SelectedChannel = value.Channels.First();
        }

        IsServerSelected = value != null;
    }

    partial void OnSelectedChannelChanged(ChannelViewModel? oldValue, ChannelViewModel? newValue)
    {
        _ = OnSelectedChannelChangedAsync(oldValue, newValue);
    }

    private async Task OnSelectedChannelChangedAsync(ChannelViewModel? oldValue, ChannelViewModel? newValue)
    {
        try
        {
            if (oldValue?.Id != null)
                await _signalRService.LeaveChannelAsync(oldValue.Id.Value);

            if (newValue?.Id != null)
            {
                await _signalRService.JoinChannelAsync(newValue.Id.Value);

                Messages.Clear();

                var token = _tokenStorageService.LoadToken();
                if (token == null) throw new Exception("Token was null");

                var fetchedMessages = await _apiService.GetMessagesForChannel(token, newValue.Id.Value);

                if (fetchedMessages != null)
                {
                    var combinedMessages = new HashSet<MessageViewModel>(newValue.Messages);

                    foreach (var fetchedMessage in fetchedMessages)
                    {
                        if (!combinedMessages.Any(m => m.Id == fetchedMessage.Id))
                            combinedMessages.Add(fetchedMessage);
                    }

                    Messages = new ObservableCollection<MessageViewModel>(
                        combinedMessages.OrderBy(m => m.Timestamp)
                    );
                }

                await UpdateOnlineUsersForCurrentChannel();
                SetChannelDisplay();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnSelectedChannelChangedAsync: {ex.Message}");
        }
    }

    private void OnMessageReceived(string user, string message, DateTime timestamp)
    {
        var newMessage = new MessageViewModel()
        {
            Username = user,
            Content = message,
            Timestamp = timestamp
        };
        Messages.Add(newMessage);
        SelectedChannel.Messages.Add(newMessage);
    }

    private async Task OnOnlineUsersUpdatedAsync(List<UserDto> userDtos)
    {
        _globalOnlineUsers = userDtos;
        await UpdateOnlineUsersForCurrentChannel();
    }

    private async Task UpdateOnlineUsersForCurrentChannel()
    {
        if (SelectedChannel == null || SelectedChannel.Id == null) return;

        var token = _tokenStorageService.LoadToken();
        if (token == null)
            throw new Exception("Token was null");

        var channelId = SelectedChannel.Id.Value;
        var usersForChannel = new List<UserDto>();

        foreach (var user in _globalOnlineUsers)
        {
            bool hasAccess = await _apiService.DoesUserHaveAccessToChannel(token, user.Id, channelId);
            if (hasAccess)
            {
                usersForChannel.Add(user);
            }
        }

        OnlineUsers = new ObservableCollection<UserViewModel>(
            usersForChannel.Select(dto => new UserViewModel { Id = dto.Id, Username = dto.Username, Nickname = dto.Nickname })
        );
    }

    private void SetChannelDisplay()
    {
        if (SelectedServer == null)
            ChannelDisplayName = "unknown";

        if (SelectedChannel == null)
            ChannelDisplayName = $"{SelectedServer.Name}/~unknown";

        ChannelDisplayName = $"{SelectedServer.Name}/#{SelectedChannel.Name}";
    }

    private void OpenServerSettings()
        => _overlayService.ShowOverlay<ServerSettingsView>();

    private void CloseOverlay()
        => _overlayService.HideOverlay();

    #endregion
}
