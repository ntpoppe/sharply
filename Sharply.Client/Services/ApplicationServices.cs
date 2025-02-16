using Sharply.Client.Interfaces;

namespace Sharply.Client.Services;

public class ApplicationServices(
    IApiService apiService,
    ITokenStorageService tokenStorageService,
    INavigationService navigationService,
    ISignalRService signalRService,
    IOverlayService overlayService,
    IServerService serverService,
    ICurrentUserService currentUserService)
{
    public IApiService ApiService { get; } = apiService;
    public ITokenStorageService TokenStorageService { get; } = tokenStorageService;
    public INavigationService NavigationService { get; } = navigationService;
    public ISignalRService SignalRService { get; } = signalRService;
    public IOverlayService OverlayService { get; } = overlayService;
    public IServerService ServerService { get; } = serverService;
    public ICurrentUserService CurrentUserService { get; } = currentUserService;
}