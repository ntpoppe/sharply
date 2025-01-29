using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;
using Sharply.Client.Services;

namespace Sharply.Client.ViewModels;

public class ServerSettingsViewModel : IOverlay
{
    private ApplicationServices _services;
	private MainViewModel _mainViewModel;

    public ServerSettingsViewModel(ApplicationServices applicationServices, MainViewModel mainViewModel)
    {
        _services = applicationServices;
		_mainViewModel = mainViewModel;
		SoftDeleteServerCommand = new AsyncRelayCommand(SoftDeleteServer);
        CloseCommand = new RelayCommand(Close);
		ServerInviteCode = GetServerInviteCode();	
    }

	public IRelayCommand SoftDeleteServerCommand { get; set; }
    public IRelayCommand CloseCommand { get; set; }
	public string ServerInviteCode { get; private set; }

	private async Task SoftDeleteServer()
	{
		var selectedServer = _mainViewModel.ServerList.SelectedServer;
		if (selectedServer == null)
			throw new InvalidOperationException("selectedServer was null in DeleteServer()");

		if (selectedServer.Id == null)
			throw new InvalidOperationException("selectedServer's id was null in DeleteServer()");

		var token = _services.TokenStorageService.LoadToken();
		if (token == null)
			throw new InvalidOperationException("Token for user was null");

		await _services.ApiService.SoftDeleteServerAsync(token, selectedServer.Id.Value);
		await _mainViewModel.RefreshServerList();
		_mainViewModel.ServerList.SelectedServer = _mainViewModel.ServerList.Servers.FirstOrDefault();
		Close();
	}

	public string GetServerInviteCode()
	{
		if (_mainViewModel.ServerList.SelectedServer == null)
			throw new InvalidOperationException("SelectedServer was null in GetServerInviteCode()");

		return _mainViewModel.ServerList.SelectedServer.InviteCode ?? "No invite code found";		
	}

    public void Close()
        => _services.OverlayService.HideOverlay();
}
