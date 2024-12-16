using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace Sharply.Client.ViewModels;

public partial class ChannelViewModel : ObservableObject
{
    [ObservableProperty]
    private string? _name;

    [ObservableProperty]
    private int? _id;

    [ObservableProperty]
    private int? _serverId;

    [ObservableProperty]
    private List<MessageViewModel> _messages = new();
}
