using System;
using System.Threading.Tasks;
using Sharply.Client.Interfaces;
using Sharply.Client.ViewModels;
using Sharply.Shared.Requests;

namespace Sharply.Client.Services;

public class ServerSerivce : IServerService
{
	private readonly ApplicationServices _services;
	private readonly MainViewModel _mainViewModel;

	public ServerSerivce(ApplicationServices services, MainViewModel mainViewModel)
	{
		_services = services;
		_mainViewModel = mainViewModel;
	}

	public async Task CreateServer(int userId, string name)
	{
		var request = new CreateServerRequest()
		{
			OwnerId = userId,
			Name = name
		};

		var token = _services.TokenStorageService.LoadToken();
		if (token == null)
			throw new InvalidOperationException("token was null");

		await _services.ApiService.CreateServerAsync(token, request);

	}
}
