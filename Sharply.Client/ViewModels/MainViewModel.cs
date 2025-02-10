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
        ChatWindow = new ChatWindowViewModel(services, ChannelList);

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

    public IRelayCommand? OpenServerSettingsCommand { get; set; }
    public IRelayCommand? OpenUserSettingsCommand { get; set; }
    public IRelayCommand? LeaveServerCommand { get; set; }

    #endregion

    #region Properties

    public string Title { get; } = "Sharply";

    [ObservableProperty]
    private CurrentUser? _displayedCurrentUser;

    [ObservableProperty]
    private string _newMessage = string.Empty;

    [ObservableProperty]
    private string? _channelDisplayName;

    public ServerListViewModel ServerList { get; set; }
    public ChannelListViewModel ChannelList { get; set; }
    public UserListViewModel UserList { get; set; }
    public ChatWindowViewModel ChatWindow { get; set; }

    public bool IsServerSelected => ServerList.IsServerSelected;
    public bool IsCurrentUserServerOwner => CheckIfCurrentServerOwner();
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
                    ChatWindow.UpdateChannelDisplay();
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

            DisplayedCurrentUser = await _services.CurrentUserService.InitializeUser(token);
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
            var currentUser = _services.CurrentUserService.CurrentUser;


            await _services.SignalRService.ConnectMessageHubAsync(token);
            await _services.SignalRService.ConnectUserHubAsync(token);

            _services.SignalRService.OnMessageReceived(ChannelList.OnMessageReceived);
            _services.SignalRService.OnOnlineUsersUpdated(users => UserList.OnOnlineUsersUpdatedAsync(users, ChannelList.SelectedChannel).Wait());

            if (currentUser == null)
                throw new Exception("CurrentUser was null.");

            await _services.SignalRService.GoOnline(currentUser.Id);

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing SignalR HubConnection: {ex.Message}");
        }
    }

    public async Task RefreshServerList()
        => await ServerList.LoadServersAsync();

    private bool CheckIfCurrentServerOwner()
    {
        var currentUser = _services.CurrentUserService.CurrentUser;
        if (currentUser == null) return false;

        return currentUser != null && ServerList.SelectedServer?.OwnerId == currentUser.Id;
    }

    private void OpenServerSettings()
        => _services.OverlayService.ShowOverlay<ServerSettingsViewModel>();

    private void OpenUserSettings()
        => _services.OverlayService.ShowOverlay<UserSettingsViewModel>();

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
            var currentUser = _services.CurrentUserService.CurrentUser;
            if (currentUser == null)
                return;

            await _services.SignalRService.DisconnectMessageHubAsync();
            await _services.SignalRService.DisconnectUserHubAsync(currentUser.Id);

            DisplayedCurrentUser = null;
            _services.CurrentUserService.ClearUser();

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
