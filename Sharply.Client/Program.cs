using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Sharply.Client.Interfaces;
using Sharply.Client.Services;
using Sharply.Client.ViewModels;
using System;


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

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<LoginViewModel>();
		services.AddSingleton<RegisterViewModel>();

        return services.BuildServiceProvider();
    }
}
