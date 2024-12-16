using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
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

    #region Constructors

    public MainViewModel(ApiService apiService, TokenStorageService tokenStorageService, INavigationService navigationService)
    {
        AddChannelCommand = new RelayCommand(AddChannel);
        SendMessageCommand = new RelayCommand(SendMessage);

        _apiService = apiService;
        _tokenStorageService = tokenStorageService;

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
    private string _channelDisplayName;

    private HubConnection? _hubConnection;

    public object? CurrentView => _navigationService.CurrentView;

    #endregion

    #region Methods

    public async Task LoadInitialData()
    {
        try
        {
            var token = _tokenStorageService.LoadToken();
            if (token == null) throw new Exception("token was null");

            var servers = await _apiService.GetServersAsync(token);
            Servers = new ObservableCollection<ServerViewModel>(servers);
            var channels = servers.SelectMany(s => s.Channels);
            Channels = new ObservableCollection<ChannelViewModel>(channels);

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private async void InitializeHubConnection()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:8001/hubs/messages")
            .Build();

        // Listen for incoming messages
        _hubConnection.On<string, string, DateTime>("ReceiveMessage", (username, content, timestamp) =>
        {
            Messages.Add(new MessageViewModel
            {
                Username = username,
                Content = content,
                Timestamp = timestamp
            });
        });

        await _hubConnection.StartAsync();
    }

    private void AddServer()
    {
    }

    private void AddChannel()
    {
    }

    private void SendMessage()
    {
        var messageToSend = NewMessage;
        Debug.WriteLine(messageToSend);
    }

    partial void OnSelectedServerChanged(ServerViewModel? value)
    {
        if (value == null) return;

        Channels.Clear();
        Channels = new ObservableCollection<ChannelViewModel>(value.Channels);
        if (Channels.Any())
            SelectedChannel = value.Channels.First();

        IsServerSelected = value != null;
    }

    partial void OnSelectedChannelChanged(ChannelViewModel? value)
    {
        if (value == null) return;

        Messages.Clear();
        if (value.Messages.Any())
            Messages = new ObservableCollection<MessageViewModel>(value.Messages);

        SetChannelDisplay();
    }

    public void SetChannelDisplay()
    {
        if (SelectedServer == null)
            ChannelDisplayName = "unknown";

        if (SelectedChannel == null)
            ChannelDisplayName = $"{SelectedServer.Name}/~unknown";

        ChannelDisplayName = $"{SelectedServer.Name}/#{SelectedChannel.Name}";
    }

    #endregion

}
