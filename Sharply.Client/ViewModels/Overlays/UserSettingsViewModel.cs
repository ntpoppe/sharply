using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sharply.Client.Interfaces;
using Sharply.Client.Services;

namespace Sharply.Client.ViewModels;

public partial class UserSettingsViewModel : ViewModelBase, IOverlay
{
    private ApplicationServices _services;

    public UserSettingsViewModel(ApplicationServices services, MainViewModel mainViewModel)
    {
        _services = services;

        LogoutCommand = new RelayCommand(mainViewModel.Logout);
        CloseCommand = new RelayCommand(Close);
        JoinServerCommand = new RelayCommand(OpenJoinServerOverlay);
        CreateServerCommand = new RelayCommand(OpenCreateServerOverlay);
        UpdateDisplayNameCommand = new RelayCommand(UpdateDisplayName);
    }

    [ObservableProperty]
    private string _newDisplayName = string.Empty;

    public IRelayCommand LogoutCommand { get; set; }
    public IRelayCommand CloseCommand { get; set; }
    public IRelayCommand JoinServerCommand { get; set; }
    public IRelayCommand CreateServerCommand { get; set; }
    public IRelayCommand UpdateDisplayNameCommand { get; set; }

    public void OpenJoinServerOverlay()
        => _services.OverlayService.ShowOverlay<JoinServerViewModel>();

    public void OpenCreateServerOverlay()
        => _services.OverlayService.ShowOverlay<CreateServerViewModel>();

    public void UpdateDisplayName()
    {

    }

    public void Close()
        => _services.OverlayService.HideOverlay();
}
