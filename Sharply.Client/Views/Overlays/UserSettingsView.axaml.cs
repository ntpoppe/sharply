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

    public UserSettingsView()
    {
        InitializeComponent();
    }
}