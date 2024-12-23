using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Sharply.Client.ViewModels;
using System;

namespace Sharply.Client.Views;

public partial class ServerSettingsView : UserControl
{
    private readonly IServiceProvider _serviceProvider;
    public ServerSettingsView(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;

        DataContext = _serviceProvider.GetRequiredService<ServerSettingsViewModel>();
    }

// Avalonia previewer
#pragma warning disable CS8618
    public ServerSettingsView() => InitializeComponent();
#pragma warning restore CS8618
}
