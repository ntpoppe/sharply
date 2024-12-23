using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Microsoft.Extensions.DependencyInjection;
using Sharply.Client.Enums;
using Sharply.Client.ViewModels;
using Sharply.Client.Views;
using System;
using System.Linq;

namespace Sharply.Client;

public partial class App : Application
{
    public static IServiceProvider? ServiceProvider;

    public App()
    {
        ServiceProvider = null;
    }

    public App(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        SetTheme(Theme.Dark);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            var mainViewModel = ServiceProvider?.GetRequiredService<MainViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    /// <summary>
    /// Switches the application theme.
    /// </summary>
    /// <param name="theme">The name of the theme to switch to (e.g., "Dark" or "Light").</param>
    public void SetTheme(Theme theme)
    {
        var themeString = Enum.GetName(typeof(Theme), theme);
        var uri = new Uri($"avares://Sharply.Client/Themes/{theme}Theme.axaml");

        Resources.MergedDictionaries.Add(new ResourceInclude(uri)
        {
            Source = uri
        });
    }
}
