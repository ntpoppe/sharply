using CommunityToolkit.Mvvm.ComponentModel;

namespace Sharply.Client.ViewModels;

public partial class UserViewModel : ObservableObject
{
    public required int Id { get; set; }

    [ObservableProperty]
    private string? _username;
}
