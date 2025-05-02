using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;
using Sharply.Client.Services;
using Sharply.Shared.Requests;

namespace Sharply.Client.ViewModels;

public partial class JoinServerViewModel : ViewModelBase, IOverlay
{
    private ApplicationServices _services;
    private MainViewModel _mainViewModel;

    public JoinServerViewModel(ApplicationServices applicationServices, MainViewModel mainViewModel)
    {
        _services = applicationServices;
        _mainViewModel = mainViewModel;
        _inviteCodeInput = string.Empty;
        _message = string.Empty;

        JoinServerCommand = new AsyncRelayCommand(JoinServer);
        CloseCommand = new RelayCommand(Close);
    }

    [ObservableProperty]
    private string _inviteCodeInput;

    [ObservableProperty]
    private string _message;

    public IRelayCommand JoinServerCommand { get; set; }
    public IRelayCommand CloseCommand { get; set; }


    public async Task JoinServer()
    {
        Console.WriteLine(InviteCodeInput);
        if (string.IsNullOrWhiteSpace(InviteCodeInput))
        {
            Message = "Invalid invite code.";
            return;
        }

        var request = new JoinServerRequest { InviteCode = InviteCodeInput };

        (int? serverId, string? error) = await _services.ServerService.JoinServerAsync(request);

        if (serverId == null)
        {
            Message = error ?? "An unknown error occurred";
            return;
        }

        await _mainViewModel.RefreshServerList();
        _mainViewModel.ServerList.SelectedServer = _mainViewModel.ServerList.Servers.Where(s => s.Id == serverId).FirstOrDefault();
        Close();
    }

    public void Close()
        => _services.OverlayService.HideOverlay();
}
