using CommunityToolkit.Mvvm.ComponentModel;

namespace Sharply.Client.ViewModels;

public partial class ChannelViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private string? _id;

    [ObservableProperty]
    private string? _serverId;
}
