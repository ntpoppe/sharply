using Sharply.Client.Interfaces;

namespace Sharply.Client.Services;

public class ApplicationServices
{
    public IApiService ApiService { get; }
    public ITokenStorageService TokenStorageService { get; }
    public INavigationService NavigationService { get; }
    public ISignalRService SignalRService { get; }
    public IOverlayService OverlayService { get; }

    public ApplicationServices(
        IApiService apiService,
        ITokenStorageService tokenStorageService,
        INavigationService navigationService,
        ISignalRService signalRService,
        IOverlayService overlayService)
    {
        ApiService = apiService;
        TokenStorageService = tokenStorageService;
        NavigationService = navigationService;
        SignalRService = signalRService;
        OverlayService = overlayService;
    }
}

