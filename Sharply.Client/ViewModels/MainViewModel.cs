using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;
using Sharply.Client.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sharply.Client.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly ApiService _apiService;
    private readonly TokenStorageService _tokenStorageService;
    private readonly INavigationService _navigationService;
    private readonly SignalRService _signalRService;


    #region Constructors

    public MainViewModel(
        ApiService apiService,
        TokenStorageService tokenStorageService,
        INavigationService navigationService,
        SignalRService signalRService)
    {
        AddChannelCommand = new RelayCommand(AddChannel);
        SendMessageCommand = new RelayCommand(SendMessage);

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
        };

        IsServerSelected = false;
    }

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
            await InitializeHubConnection(token);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
    public async Task InitializeHubConnection(string token)
    {
        try
        {
            await _signalRService.ConnectAsync(token);

            _signalRService.OnMessageReceived((user, message, timestamp) =>
            {
                Console.WriteLine($"Received message from user {user}: {message}");
                Messages.Add(new MessageViewModel
                {
                    Username = user,
                    Content = message,
                    Timestamp = timestamp
                });
            });

            Console.WriteLine("Subscribed to ReceiveMessage event.");
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

                if (newValue.Messages.Any())
                    Messages = new ObservableCollection<MessageViewModel>(newValue.Messages);

                SetChannelDisplay();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnSelectedChannelChangedAsync: {ex.Message}");
        }
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
