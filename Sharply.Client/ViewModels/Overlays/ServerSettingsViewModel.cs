using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;

namespace Sharply.Client.ViewModels;

public class ServerSettingsViewModel : IOverlay
{
    private IOverlayService _overlayService;

    public ServerSettingsViewModel(IOverlayService overlayService)
    {
        _overlayService = overlayService;
        CloseCommand = new RelayCommand(Close);
    }

    public IRelayCommand CloseCommand { get; set; }

    public void Close()
        => _overlayService.HideOverlay();
}
