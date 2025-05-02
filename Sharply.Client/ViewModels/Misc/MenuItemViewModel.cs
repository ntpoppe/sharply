using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Sharply.Client.ViewModels;

#pragma warning disable CS8618

public partial class MenuItemViewModel : ViewModelBase
{
    public MenuItemViewModel()
        => Items = new ObservableCollection<MenuItemViewModel>();

    [ObservableProperty]
    private string _header;

    [ObservableProperty]
    private IRelayCommand _command;

    [ObservableProperty]
    private object _commandParameter;

    [ObservableProperty]
    private ObservableCollection<MenuItemViewModel> _items;
}
