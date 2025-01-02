using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;
using Sharply.Client.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Sharply.Client.ViewModels;

public partial class MainViewModel : ViewModelBase, INavigable
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

		ServerList = new ServerListViewModel(_apiService, _tokenStorageService);
		ChannelList = new ChannelListViewModel(_apiService, _signalRService, _tokenStorageService);
		UserList = new UserListViewModel(_apiService, _tokenStorageService);

        InitializeEvents();
        InitializeCommands();

        if (_navigationService.CurrentView == null)
            _navigationService.NavigateTo<LoginViewModel>();

        IsServerSelected = false;
    }

// Avalonia previewer
#pragma warning disable CS8618
    public MainViewModel() { }
#pragma warning restore CS8618

    #endregion

    #region Commands
    public IRelayCommand? AddChannelCommand { get; set; }
    public IRelayCommand? SendMessageCommand { get; set; }
    public IRelayCommand? OpenServerSettingsCommand { get; set; }
    public IRelayCommand? OpenUserSettingsCommand { get; set; }
    #endregion

    #region Properties

    public string Title { get; } = "Sharply";

    [ObservableProperty]
    private CurrentUser? _currentUser;


    [ObservableProperty]
    private bool _isServerSelected;

    [ObservableProperty]
    private string _newMessage = string.Empty;

    [ObservableProperty]
    private string? _channelDisplayName;

	/* NEW VIEW MODELS */
	public ServerListViewModel ServerList { get; set; }
	public ChannelListViewModel ChannelList { get; set; }
	public UserListViewModel UserList { get; set; }

    public object? CurrentView => _navigationService.CurrentView;
    public object? CurrentOverlay => _overlayService.CurrentOverlayView;
    public bool IsOverlayVisible => _overlayService.IsOverlayVisible;


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
        OpenUserSettingsCommand = new RelayCommand(OpenUserSettings);
    }

    public async Task LoadInitialData()
    {
        try
        {
            var token = _tokenStorageService.LoadToken();
            if (token == null) throw new Exception("token was null");

            var userData = await _apiService.GetCurrentUserData(token);
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
            await _signalRService.ConnectMessageHubAsync(token);
            await _signalRService.ConnectUserHubAsync(token);

            _signalRService.OnMessageReceived(ChannelList.OnMessageReceived);
            _signalRService.OnOnlineUsersUpdated(users => UserList.OnOnlineUsersUpdatedAsync(users, ChannelList.SelectedChannel).Wait());

			if (CurrentUser == null)
				throw new Exception("CurrentUser was null. You should never see this.");

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
            if (CurrentUser == null || ChannelList.SelectedChannel == null || ChannelList.SelectedChannel.Id == null || string.IsNullOrWhiteSpace(NewMessage))
                return;

            await _signalRService.SendMessageAsync(ChannelList.SelectedChannel.Id.Value, CurrentUser.Id, NewMessage);
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
			ChannelDisplayName = $"{ServerList.SelectedServer?.Name}/~unknown";
			return;
		}

		ChannelDisplayName = $"{ServerList.SelectedServer?.Name}/#{ChannelList.SelectedChannel?.Name}";
	}


    private void OpenServerSettings()
        => _overlayService.ShowOverlay<ServerSettingsViewModel>();

    private void OpenUserSettings()
    {
        var view = CurrentView;
        _overlayService.ShowOverlay<UserSettingsViewModel>();
    }

    private void CloseOverlay()
        => _overlayService.HideOverlay();

    public async void Logout()
    {
        try
        {
			if (CurrentUser == null)
				return;

            await _signalRService.DisconnectMessageHubAsync();
            await _signalRService.DisconnectUserHubAsync(CurrentUser.Id);

            CurrentUser = null;
            _tokenStorageService.ClearToken();
            _overlayService.HideOverlay();
            _navigationService.NavigateTo<LoginViewModel>();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error during logout: {ex}");
        }
    }

    #endregion
}
