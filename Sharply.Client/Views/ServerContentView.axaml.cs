using Avalonia.Controls;
using Avalonia.Input;
using Sharply.Client.ViewModels;

namespace Sharply.Client.Views;

public partial class ServerContentView : UserControl
{
    public ServerContentView()
    {
        InitializeComponent();
    }

	private void MessageInput_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.Key == Key.Enter)
		{
			if (DataContext is MainViewModel viewModel)
			{
				viewModel.SendMessageCommand.Execute(null);
			}
		}
	}
}
