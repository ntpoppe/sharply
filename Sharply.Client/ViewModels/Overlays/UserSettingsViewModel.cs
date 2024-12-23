using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;

namespace Sharply.Client.ViewModels;

public class UserSettingsViewModel
{
    private IOverlayService _overlayService;

    public UserSettingsViewModel(IOverlayService overlayService, MainViewModel mainViewModel)
    {
        _overlayService = overlayService;
        LogoutCommand = new RelayCommand(mainViewModel.Logout);
        CloseCommand = new RelayCommand(Close);
    }

    public IRelayCommand LogoutCommand { get; set; }
    public IRelayCommand CloseCommand { get; set; }

    public void Close()
        => _overlayService.HideOverlay();
}
