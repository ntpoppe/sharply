using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Sharply.Client.ViewModels;
using System;

namespace Sharply.Client.Views;

public partial class CreateServerView : UserControl
{
    private readonly IServiceProvider _serviceProvider;
    public CreateServerView(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;

        DataContext = _serviceProvider.GetRequiredService<CreateServerViewModel>();
    }

// Avalonia previewer
#pragma warning disable CS8618
    public CreateServerView() => InitializeComponent();
#pragma warning restore CS8618
}
