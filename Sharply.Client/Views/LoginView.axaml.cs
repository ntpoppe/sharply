using Avalonia.Input;
using Avalonia.Controls;
using Sharply.Client.ViewModels;

namespace Sharply.Client.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        InitializeComponent();
    }

	private void PasswordBox_KeyDown(object? sender, KeyEventArgs e)
	{
		if (e.Key == Key.Enter)
		{
			if (DataContext is LoginViewModel viewModel)
			{
				viewModel.LoginCommand.Execute(null);
			}
		}
	}

}
