using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;
using Sharply.Client.Services;

namespace Sharply.Client.ViewModels;

public class UserSettingsViewModel : IOverlay
{
    private ApplicationServices _services;

    public UserSettingsViewModel(ApplicationServices services, MainViewModel mainViewModel)
    {
		_services = services;	

        LogoutCommand = new RelayCommand(mainViewModel.Logout);
        CloseCommand = new RelayCommand(Close);
		JoinServerCommand = new RelayCommand(OpenJoinServerOverlay);
		CreateServerCommand = new RelayCommand(OpenCreateServerOverlay);
    }

    public IRelayCommand LogoutCommand { get; set; }
    public IRelayCommand CloseCommand { get; set; }
	public IRelayCommand JoinServerCommand { get; set; }
	public IRelayCommand CreateServerCommand { get; set; }

	public void OpenJoinServerOverlay()
		=> _services.OverlayService.ShowOverlay<JoinServerViewModel>();

	public void OpenCreateServerOverlay()
		=> _services.OverlayService.ShowOverlay<CreateServerViewModel>();

	public void Close()
        => _services.OverlayService.HideOverlay();
}
