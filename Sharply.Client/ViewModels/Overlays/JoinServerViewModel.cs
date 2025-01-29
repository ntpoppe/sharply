using System;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
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

		var token = _services.TokenStorageService.LoadToken();
		if (token == null)
			throw new InvalidOperationException("Token was not found in JoinServer()");

		var request = new JoinServerRequest { InviteCode = InviteCodeInput };

		var apiResponse = await _services.ApiService.JoinServerAsync(token, request);

		if (!apiResponse.Success)
		{
			Message = apiResponse.Error ?? "An unknown error occurred.";
			return;
		}

		await _mainViewModel.RefreshServerList();
		_mainViewModel.ServerList.SelectedServer = _mainViewModel.ServerList.Servers.Where(s => s.Id == apiResponse.Data.Id).FirstOrDefault();
		Close();	
	}
	
    public void Close()
        => _services.OverlayService.HideOverlay();
}
