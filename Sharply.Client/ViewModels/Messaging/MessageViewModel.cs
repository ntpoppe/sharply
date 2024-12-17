using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Sharply.Client.ViewModels;

public partial class MessageViewModel : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private string? _username;

    [ObservableProperty]
    private string? _content;

    [ObservableProperty]
    private DateTime? _timestamp;
}

