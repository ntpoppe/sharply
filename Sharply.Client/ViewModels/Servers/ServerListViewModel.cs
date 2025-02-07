using CommunityToolkit.Mvvm.ComponentModel;
using Sharply.Client.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Sharply.Client.ViewModels;

public partial class ServerListViewModel : ViewModelBase
{
    private IApiService _apiService;
    private ITokenStorageService _tokenStorageService;

    public ServerListViewModel(IApiService apiService, ITokenStorageService tokenStorageService)
    {
        _apiService = apiService;
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
        var token = _tokenStorageService.LoadToken();
        if (token == null)
            throw new InvalidOperationException("Token was not found.");

        var fetchedServers = await _apiService.GetServersAsync(token);
        Servers = new ObservableCollection<ServerViewModel>(fetchedServers);
    }

    partial void OnSelectedServerChanged(ServerViewModel? value)
        => IsServerSelected = value != null;

}
