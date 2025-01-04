using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;

namespace Sharply.Client.ViewModels;

public class UserSettingsViewModel : IOverlay
{
    private IOverlayService _overlayService;

    public UserSettingsViewModel(IOverlayService overlayService, MainViewModel mainViewModel)
    {
        _overlayService = overlayService;
        LogoutCommand = new RelayCommand(mainViewModel.Logout);
        CloseCommand = new RelayCommand(Close);
		JoinServerCommand = new RelayCommand(Close);
		CreateServerCommand = new RelayCommand(Close);
    }

    public IRelayCommand LogoutCommand { get; set; }
    public IRelayCommand CloseCommand { get; set; }
	public IRelayCommand JoinServerCommand { get; set; }
	public IRelayCommand CreateServerCommand { get; set; }

	public void Close()
        => _overlayService.HideOverlay();
}
