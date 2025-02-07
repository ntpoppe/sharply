using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;
using Sharply.Client.Models;
using Sharply.Client.Services;
using Sharply.Shared.Requests;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sharply.Client.ViewModels;

public partial class MainViewModel : ViewModelBase, INavigable
{
    private readonly ApplicationServices _services;

    #region Constructors

    public MainViewModel(ApplicationServices services)
    {
        _services = services;

        ServerList = new ServerListViewModel(services.ApiService, services.TokenStorageService);
        ChannelList = new ChannelListViewModel(services.ApiService, services.SignalRService, services.TokenStorageService);
        UserList = new UserListViewModel(services.ApiService, services.TokenStorageService);

        InitializeEvents();
        InitializeCommands();

        if (services.NavigationService.CurrentView == null)
            services.NavigationService.NavigateTo<LoginViewModel>();
    }

    // Avalonia previewer
#pragma warning disable CS8618
    public MainViewModel() { }
#pragma warning restore CS8618

    #endregion

    #region Commands

    public IRelayCommand? SendMessageCommand { get; set; }
    public IRelayCommand? OpenServerSettingsCommand { get; set; }
    public IRelayCommand? OpenUserSettingsCommand { get; set; }

    public IRelayCommand? LeaveServerCommand { get; set; }

    #endregion

    #region Properties

    public string Title { get; } = "Sharply";

    [ObservableProperty]
    private CurrentUser? _currentUser;

    [ObservableProperty]
    private string _newMessage = string.Empty;

    [ObservableProperty]
    private string? _channelDisplayName;

    public ServerListViewModel ServerList { get; set; }
    public ChannelListViewModel ChannelList { get; set; }
    public UserListViewModel UserList { get; set; }

    public bool IsServerSelected => ServerList.IsServerSelected;
    public bool IsCurrentUserServerOwner => CurrentUser != null && ServerList.SelectedServer?.OwnerId == CurrentUser.Id;
    public object? CurrentView => _services.NavigationService.CurrentView;
    public object? CurrentOverlay => _services.OverlayService.CurrentOverlayView;
    public bool IsOverlayVisible => _services.OverlayService.IsOverlayVisible;

    public ObservableCollection<MenuItemViewModel> ServerMenuItems => IsCurrentUserServerOwner
        ? new ObservableCollection<MenuItemViewModel>
        {
            new MenuItemViewModel { Header = "Settings", Command = OpenServerSettingsCommand ?? new RelayCommand(OpenServerSettings) },
            new MenuItemViewModel { Header = "Leave Server", Command = LeaveServerCommand ?? new RelayCommand(OpenServerSettings) }
        }
        : new ObservableCollection<MenuItemViewModel>
        {
            new MenuItemViewModel { Header = "Leave Server", Command = LeaveServerCommand ?? new RelayCommand(OpenServerSettings) }
        };

    #endregion

    #region Methods

    public void OnNavigatedTo(object? parameter)
    {
        // Do nothing, may be useful in the future;
    }

    public void InitializeEvents()
    {
        ServerList.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ServerList.SelectedServer))
            {
                var selectedServer = ServerList.SelectedServer;
                if (selectedServer != null)
                    ChannelList.LoadChannelsAsync(selectedServer);

                OnPropertyChanged(nameof(IsServerSelected));
                OnPropertyChanged(nameof(IsCurrentUserServerOwner));
                OnPropertyChanged(nameof(ServerMenuItems));
            }
        };

        ChannelList.PropertyChanged += async (s, e) =>
        {
            if (e.PropertyName == nameof(ChannelList.SelectedChannel))
            {
                var selectedChannel = ChannelList.SelectedChannel;
                if (selectedChannel != null)
                {
                    SetChannelDisplay();
                    await UserList.UpdateOnlineUsersForCurrentChannel(selectedChannel);
                }
            }
        };

        _services.OverlayService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_services.OverlayService.IsOverlayVisible))
                OnPropertyChanged(nameof(IsOverlayVisible));

            if (e.PropertyName == nameof(_services.OverlayService.CurrentOverlayView))
                OnPropertyChanged(nameof(CurrentOverlay));
        };

        _services.NavigationService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_services.NavigationService.CurrentView))
                OnPropertyChanged(nameof(CurrentView));
        };
    }

    public void InitializeCommands()
    {
        SendMessageCommand = new RelayCommand(SendMessage);
        OpenServerSettingsCommand = new RelayCommand(OpenServerSettings);
        OpenUserSettingsCommand = new RelayCommand(OpenUserSettings);
        LeaveServerCommand = new AsyncRelayCommand(LeaveServer);
    }

    public async Task LoadInitialData()
    {
        try
        {
            var token = _services.TokenStorageService.LoadToken();
            if (token == null) throw new Exception("token was null");

            var userData = await _services.ApiService.GetCurrentUserData(token);
            if (userData == null)
                throw new Exception("User data missing.");

            CurrentUser = CurrentUser.FromDto(userData);

            await ServerList.LoadServersAsync();
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
            await _services.SignalRService.ConnectMessageHubAsync(token);
            await _services.SignalRService.ConnectUserHubAsync(token);

            _services.SignalRService.OnMessageReceived(ChannelList.OnMessageReceived);
            _services.SignalRService.OnOnlineUsersUpdated(users => 
			{
			    if (ChannelList.SelectedChannel != null)
				{
					UserList.OnOnlineUsersUpdatedAsync(users, ChannelList.SelectedChannel).Wait();
				}			
			});

            if (CurrentUser == null)
                throw new Exception("CurrentUser was null.");

            await _services.SignalRService.GoOnline(CurrentUser.Id);

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing SignalR HubConnection: {ex.Message}");
        }
    }

    public async Task RefreshServerList()
        => await ServerList.LoadServersAsync();

    private async void SendMessage()
    {
        try
        {
            if (CurrentUser == null || ChannelList.SelectedChannel == null || ChannelList.SelectedChannel.Id == null || string.IsNullOrWhiteSpace(NewMessage))
                return;

            await _services.SignalRService.SendMessageAsync(ChannelList.SelectedChannel.Id.Value, CurrentUser.Id, NewMessage);
            NewMessage = string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    private void SetChannelDisplay()
    {
        if (ServerList.SelectedServer == null)
        {
            ChannelDisplayName = "unknown";
            return;
        }

        if (ChannelList.SelectedChannel == null)
        {
            ChannelDisplayName = $"{ServerList.SelectedServer?.Name} - ~unknown";
            return;
        }

        ChannelDisplayName = $"{ServerList.SelectedServer?.Name} / {ChannelList.SelectedChannel?.Name}";
    }


    private void OpenServerSettings()
        => _services.OverlayService.ShowOverlay<ServerSettingsViewModel>();

    private void OpenUserSettings()
    {
        var view = CurrentView;
        _services.OverlayService.ShowOverlay<UserSettingsViewModel>();
    }

    private async Task LeaveServer()
    {
        var token = _services.TokenStorageService.LoadToken();
        if (token == null) throw new Exception("token was null");

        var serverId = ServerList.SelectedServer.Id;
        if (serverId == null) return;

        var request = new LeaveServerRequest() { ServerId = serverId.Value };

        var result = await _services.ApiService.LeaveServerAsync(token, request);
        if (result.Success)
        {
            await RefreshServerList();
            ServerList.SelectedServer = null;
        }
    }

    public async void Logout()
    {
        try
        {
            if (CurrentUser == null)
                return;

            await _services.SignalRService.DisconnectMessageHubAsync();
            await _services.SignalRService.DisconnectUserHubAsync(CurrentUser.Id);

            CurrentUser = null;
            _services.TokenStorageService.ClearToken();
            _services.OverlayService.HideOverlay();
            _services.NavigationService.NavigateTo<LoginViewModel>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error during logout: {ex}");
        }
    }

    #endregion
}
