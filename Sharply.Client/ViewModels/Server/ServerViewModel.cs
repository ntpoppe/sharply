using CommunityToolkit.Mvvm.ComponentModel;

namespace Sharply.Client.ViewModels;

public partial class ServerViewModel : ObservableObject
{
	[ObservableProperty]
	private string? _name;
}
