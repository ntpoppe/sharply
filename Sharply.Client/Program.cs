using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Sharply.Client.Interfaces;
using Sharply.Client.Services;
using Sharply.Client.ViewModels;
using System;
using System.Net.Http;


namespace Sharply.Client;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var serviceProvider = ConfigureServices();
        BuildAvaloniaApp(serviceProvider).StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp(IServiceProvider serviceProvider)
       => AppBuilder.Configure(() => new App(serviceProvider))
            .UsePlatformDetect()
            .LogToTrace();

    // Parameterless BuildAvaloniaApp for the previewer
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<TokenStorageService>();
        services.AddSingleton<HttpClient>(provider =>
        {
            var handler = new HttpClientHandler
            {
                // TODO: Disable this in production
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            return new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost:8001/")
            };
        });
        services.AddSingleton<ApiService>();

        services.AddTransient<MainViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();

        return services.BuildServiceProvider();
    }
}
