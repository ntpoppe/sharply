using Avalonia.Controls;
using Avalonia.Input;
using Sharply.Client.ViewModels;

namespace Sharply.Client.Views;

public partial class ChatWindowControl : UserControl
{
    public ChatWindowControl()
    {
        InitializeComponent();
    }

    private void MessageInput_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (DataContext is ChatWindowViewModel viewModel)
            {
                viewModel.SendMessageCommand.Execute(null);
            }
        }
    }
}