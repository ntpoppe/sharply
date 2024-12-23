using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Sharply.Client.ViewModels;
using System;

namespace Sharply.Client.Views;

public partial class UserSettingsView : UserControl
{
    private readonly IServiceProvider _serviceProvider;
    public UserSettingsView(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;

        DataContext = _serviceProvider.GetRequiredService<UserSettingsViewModel>();
    }

// Avalonia previewer
#pragma warning disable CS8618
    public UserSettingsView() => InitializeComponent();
#pragma warning restore CS8618
}
