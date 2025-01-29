using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sharply.Client.Interfaces;
using Sharply.Client.Services;
using Sharply.Client.ViewModels;
using Sharply.Client.AutoMapper;
using System;
using System.IO;
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
        var configuration = LoadConfiguration();
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ITokenStorageService, TokenStorageService>();
        services.AddSingleton<IApiService, ApiService>();
        services.AddSingleton<IOverlayService, OverlayService>();
		services.AddSingleton<IServerService, ServerSerivce>();
        services.AddSingleton<HttpClient>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var serverUri = config["ServerSettings:ServerUri"] ?? "http://localhost:8000";
            var handler = new HttpClientHandler();

            return new HttpClient(handler)
            {
                BaseAddress = new Uri(serverUri)
            };
        });
        services.AddSingleton<ISignalRService>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var serverUri = config["ServerSettings:ServerUri"] ?? "http://localhost:8000";
            return new SignalRService(serverUri);
        });
		services.AddSingleton<ApplicationServices>();
        services.AddSingleton<MainViewModel>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<ServerSettingsViewModel>();
        services.AddTransient<UserSettingsViewModel>();
		services.AddTransient<CreateServerViewModel>();
		services.AddTransient<JoinServerViewModel>();

		services.AddAutoMapper(typeof(MappingProfile));

        return services.BuildServiceProvider();
    }

    private static IConfiguration LoadConfiguration()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string projectDirectory = FindProjectDirectory(baseDirectory, "Sharply.Client");
        DotNetEnv.Env.Load(Path.Combine(projectDirectory, ".env"));

        var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        return builder.Build();
    }

    private static string FindProjectDirectory(string startDirectory, string projectName)
    {
        var currentDirectory = new DirectoryInfo(startDirectory);

        while (currentDirectory != null)
        {
            if (currentDirectory.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase))
            {
                return currentDirectory.FullName;
            }
            currentDirectory = currentDirectory.Parent;
        }

        return string.Empty;
    }
}
