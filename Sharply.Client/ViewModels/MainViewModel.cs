using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using Sharply.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Sharply.Client.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly INavigationService NavigationService;

    #region Constructors

    public MainViewModel(INavigationService navigationService)
    {
        SelectServerCommand = new RelayCommand<ServerViewModel>(async (server) => await SelectServer(server));
        AddServerCommand = new RelayCommand(AddServer);
        SelectChannelCommand = new RelayCommand<ChannelViewModel>(SelectChannel);
        AddChannelCommand = new RelayCommand(AddChannel);
        SendMessageCommand = new RelayCommand(SendMessage);

        NavigationService = navigationService;
        NavigationService.NavigateTo<LoginViewModel>();
        NavigationService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(NavigationService.CurrentView))
            {
                OnPropertyChanged(nameof(CurrentView));
            }
        };

        LoadInitialData();
    }

    #endregion

    #region Commands
    public IRelayCommand SelectServerCommand { get; }
    public IRelayCommand AddServerCommand { get; }
    public IRelayCommand SelectChannelCommand { get; }
    public IRelayCommand AddChannelCommand { get; }
    public IRelayCommand SendMessageCommand { get; }
    #endregion

    #region Properties

    public string Title { get; } = "Sharply";

    // Collections
    [ObservableProperty]
    private ObservableCollection<ServerViewModel> _servers = new();

    [ObservableProperty]
    private ServerViewModel? _selectedServer;

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

    private HubConnection _hubConnection;

    public object? CurrentView => NavigationService.CurrentView;

    #endregion

    #region Methods

    private void LoadInitialData()
    {
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

    private async Task SelectServer(ServerViewModel server)
    {
        SelectedServer = server;
        Channels.Clear();

        var channels = await _hubConnection.InvokeAsync<List<ChannelDto>>("GetChannelsForServer", server.Id, "currentUserId");
        foreach (var channel in channels)
        {
            Channels.Add(new ChannelViewModel { Id = channel.Id, Name = channel.Name, ServerId = server.Id });
        }
    }

    private void AddServer()
    {
    }

    private void SelectChannel(ChannelViewModel channel)
    {
    }

    private void AddChannel()
    {
    }

    private void SendMessage()
    {
    }

    #endregion

}
