using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sharply.Client.ViewModels;

public partial class MessageViewModel : ObservableObject
{
	[ObservableProperty]
	private string? _username;

	[ObservableProperty]
	private string? _content;

	[ObservableProperty]
	private DateTime? _timestamp;
}

