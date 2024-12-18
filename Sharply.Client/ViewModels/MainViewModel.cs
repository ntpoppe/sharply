using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;
using Sharply.Client.Services;
using Sharply.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sharply.Client.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private readonly TokenStorageService _tokenStorageService;
    private readonly INavigationService _navigationService;
    private readonly SignalRService _signalRService;

    public event PropertyChangedEventHandler? PropertyChangedWindow;

    #region Constructors

    public MainViewModel(
        IApiService apiService,
        TokenStorageService tokenStorageService,
        INavigationService navigationService,
        SignalRService signalRService)
    {
        _apiService = apiService;
        _tokenStorageService = tokenStorageService;
        _signalRService = signalRService;

        _navigationService = navigationService;
        _navigationService.NavigateTo<LoginViewModel>();
        _navigationService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_navigationService.CurrentView))
            {
                OnPropertyChanged(nameof(CurrentView));
            }

            if (e.PropertyName == nameof(_navigationService.IsOverlayVisible))
            {
                OnPropertyChanged(nameof(IsOverlayVisible));
            }
        };

        AddChannelCommand = new RelayCommand(AddChannel);
        SendMessageCommand = new RelayCommand(SendMessage);
        TestToggleOverlayCommand = new RelayCommand(() =>
        {
            _navigationService.SetOverlayVisible(!IsOverlayVisible);
        });

        IsServerSelected = false;
    }

    public MainViewModel() { }

    #endregion

    #region Commands
    public IRelayCommand AddChannelCommand { get; }
    public IRelayCommand SendMessageCommand { get; }
    #endregion

    #region Properties

    public string Title { get; } = "Sharply";

    /* TODO: Make these into it's own class, I'm sure this is going to be extended */
    private int? CurrentUserId { get; set; }
    private string? CurrentUserName { get; set; }

    /* ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ */

    public IRelayCommand TestToggleOverlayCommand { get; }

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

    public bool IsOverlayVisible => _navigationService.IsOverlayVisible;

    public object? CurrentView => _navigationService.CurrentView;

    private List<UserDto> _globalOnlineUsers = new();

    #endregion

    #region Methods

    public async Task LoadInitialData()
    {
        try
        {
            // Get the token, and parse the data from it
            var token = _tokenStorageService.LoadToken();
            if (token == null) throw new Exception("token was null");

            var tokenData = await _apiService.GetCurrentUserTokenData(token);
            if (tokenData == null)
                throw new Exception("Token data missing.");

            CurrentUserId = tokenData.UserId;
            CurrentUserName = tokenData.Username;

            // Retrieve all servers and channels associated with the user
            var servers = await _apiService.GetServersAsync(token);
            Servers = new ObservableCollection<ServerViewModel>(servers);

            var channels = servers.SelectMany(s => s.Channels);
            Channels = new ObservableCollection<ChannelViewModel>(channels);

            // Initialize SignalR hub connection
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

            if (CurrentUserId.HasValue)
                await _signalRService.GoOnline(CurrentUserId.Value);

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
            if (SelectedChannel?.Id == null || CurrentUserId == null || string.IsNullOrWhiteSpace(NewMessage))
                return;

            await _signalRService.SendMessageAsync(SelectedChannel.Id.Value, CurrentUserId.Value, NewMessage);
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

                await Task.Delay(1000);

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
            usersForChannel.Select(dto => new UserViewModel { Id = dto.Id, Username = dto.Username })
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

    #endregion
}
