using System;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Sharply.Client.ViewModels.Overlays;

namespace Sharply.Client.Views.Overlays;

public partial class CreateServerView : UserControl
{
    public CreateServerView(IServiceProvider serviceProvider)
    {
        InitializeComponent();

        DataContext = serviceProvider.GetRequiredService<CreateServerViewModel>();
    }

    // Avalonia previewer
#pragma warning disable CS8618
    public CreateServerView() => InitializeComponent();
#pragma warning restore CS8618
}
