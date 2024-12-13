using CommunityToolkit.Mvvm.ComponentModel;

namespace Sharply.Client.ViewModels;

public partial class UserViewModel : ObservableObject
{
	[ObservableProperty]
	private string? _username;
}
