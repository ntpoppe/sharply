using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Sharply.Client.Interfaces;

namespace Sharply.Client.ViewModels;

public partial class ServerListViewModel : ViewModelBase
{
    private IServerService _serverService;
    private ITokenStorageService _tokenStorageService;

    public ServerListViewModel(IServerService serverService, ITokenStorageService tokenStorageService)
    {
        _serverService = serverService;
        _tokenStorageService = tokenStorageService;

        IsServerSelected = false;
    }

    [ObservableProperty]
    private ObservableCollection<ServerViewModel> _servers = new();

    [ObservableProperty]
    private ServerViewModel? _selectedServer;

    [ObservableProperty]
    private bool _isServerSelected = false;

    public async Task LoadServersAsync()
    {
        var fetchedServers = await _serverService.GetServersAsync();
        Servers = new ObservableCollection<ServerViewModel>(fetchedServers);
    }

    public string GetLoadedServerName(int id)
    {
        var server = Servers.FirstOrDefault(s => s.Id == id);
        if (server == null) throw new InvalidOperationException("server was null in GetLoadedServerName()");

        return server.Name ?? "Unknown";
    }

    partial void OnSelectedServerChanged(ServerViewModel? value)
        => IsServerSelected = value != null;

}
