using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sharply.Client.ViewModels;

public partial class ServerViewModel : ObservableObject
{
    [ObservableProperty]
    private int? _id;

    [ObservableProperty]
    private int? _ownerId;

    [ObservableProperty]
    private string? _name;

    public required string InviteCode { get; set; }

    [ObservableProperty]
    private List<ChannelViewModel> _channels = new();
}
