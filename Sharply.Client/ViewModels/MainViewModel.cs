using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;
using System.Collections.ObjectModel;

namespace Sharply.Client.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly INavigationService NavigationService;

    #region Constructors

    public MainViewModel(INavigationService navigationService)
    {
        SelectServerCommand = new RelayCommand<ServerViewModel>(SelectServer);
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
    private ObservableCollection<ServerViewModel> _servers = new ObservableCollection<ServerViewModel>();

    [ObservableProperty]
    private ServerViewModel? _selectedServer;

    [ObservableProperty]
    private ObservableCollection<ChannelViewModel> _channels = new ObservableCollection<ChannelViewModel>();

    [ObservableProperty]
    private ChannelViewModel? _selectedChannel;

    [ObservableProperty]
    private ObservableCollection<MessageViewModel> _messages = new ObservableCollection<MessageViewModel>();

    [ObservableProperty]
    private ObservableCollection<UserViewModel> _onlineUsers = new ObservableCollection<UserViewModel>();

    [ObservableProperty]
    private string _newMessage = string.Empty;

    #endregion

    #region Methods

    private void LoadInitialData()
    {
        // Add a default global server
        var globalServer = new ServerViewModel { Name = "Global" };
        Servers.Add(globalServer);
        SelectedServer = globalServer;

        // Add a default global channel
        var globalChannel = new ChannelViewModel { Name = "General" };
        Channels.Add(globalChannel);
        SelectedChannel = globalChannel;

        // Add some default online users
        OnlineUsers.Add(new UserViewModel { Username = "Alice" });
        OnlineUsers.Add(new UserViewModel { Username = "Bob" });

        // Add some default messages
        Messages.Add(new MessageViewModel { Username = "Alice", Content = "Welcome to Sharply!" });
        Messages.Add(new MessageViewModel { Username = "Bob", Content = "Hello everyone!" });
    }

    private void SelectServer(ServerViewModel server)
    {
        SelectedServer = server;
        // Load channels for the selected server
        Channels.Clear();
        Channels.Add(new ChannelViewModel { Name = "General" });
        SelectedChannel = Channels[0];
    }

    private void AddServer()
    {
        // Logic to add a new server
        var newServer = new ServerViewModel { Name = "New Server" };
        Servers.Add(newServer);
        SelectedServer = newServer;
    }

    private void SelectChannel(ChannelViewModel channel)
    {
        SelectedChannel = channel;
        // Load messages and online users for the selected channel
        Messages.Clear();
        OnlineUsers.Clear();

        // Simulate loading messages and users
        Messages.Add(new MessageViewModel { Username = "Alice", Content = "Welcome to the new channel!" });
        OnlineUsers.Add(new UserViewModel { Username = "Alice" });
    }

    private void AddChannel()
    {
        // Logic to add a new channel
        var newChannel = new ChannelViewModel { Name = "New Channel" };
        Channels.Add(newChannel);
        SelectedChannel = newChannel;
    }

    private void SendMessage()
    {
        if (!string.IsNullOrWhiteSpace(NewMessage))
        {
            Messages.Add(new MessageViewModel { Username = "You", Content = NewMessage });
            NewMessage = string.Empty;

            // Scroll to the latest message if using ScrollViewer
        }
    }

    #endregion

    #region View Models, Move to own file eventually

    public partial class ServerViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;
    }

    public partial class ChannelViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;
    }

    public partial class MessageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _content = string.Empty;
    }

    public partial class UserViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _username = string.Empty;
    }

    public object? CurrentView => NavigationService.CurrentView;
    #endregion
}
