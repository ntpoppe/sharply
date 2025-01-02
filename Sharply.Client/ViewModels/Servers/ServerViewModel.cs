using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace Sharply.Client.ViewModels;

public partial class ServerViewModel : ObservableObject
{
    [ObservableProperty]
    private int? _id;

    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private List<ChannelViewModel> _channels = new();
}
