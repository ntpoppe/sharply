using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;
using Sharply.Client.Services;

namespace Sharply.Client.ViewModels.Overlays;

public partial class CreateServerViewModel : ViewModelBase, IOverlay
{
    private readonly ApplicationServices _services;
    private readonly MainViewModel _mainViewModel;

    public CreateServerViewModel(ApplicationServices services, MainViewModel mainViewModel)
    {
        _services = services;
        _mainViewModel = mainViewModel;

        CloseCommand = new RelayCommand(Close);
        CreateServerCommand = new AsyncRelayCommand(CreateServer);
    }

    public IRelayCommand CloseCommand { get; set; }
    public IAsyncRelayCommand CreateServerCommand { get; set; }

    [ObservableProperty]
    private string _newServerName = string.Empty;

    private async Task CreateServer()
    {
        if (string.IsNullOrWhiteSpace(NewServerName))
            return;

        var user = await _services.UserService.GetCurrentUserDataAsync();
        if (user == null)
            throw new InvalidOperationException("user was null");

        var newServer = await _services.ServerService.CreateServerAsync(user.Id, NewServerName);
        await _mainViewModel.RefreshServerList();
        _mainViewModel.ServerList.SelectedServer = newServer;
        Close();
    }

    public void Close()
        => _services.OverlayService.HideOverlay();
}
