using System;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Sharply.Client.ViewModels.Overlays;

namespace Sharply.Client.Views.Overlays;

public partial class UserSettingsView : UserControl
{
    public UserSettingsView(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        DataContext = serviceProvider.GetRequiredService<UserSettingsViewModel>();
    }

    // Avalonia previewer
#pragma warning disable CS8618
    public UserSettingsView() => InitializeComponent();
#pragma warning restore CS8618
}
