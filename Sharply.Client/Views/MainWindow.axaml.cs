using Avalonia.Controls;

namespace Sharply.Client.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent(loadXaml: true, attachDevTools: true);
    }
}