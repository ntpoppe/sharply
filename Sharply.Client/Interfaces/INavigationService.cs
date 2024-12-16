using System.ComponentModel;

namespace Sharply.Client.Interfaces;

public interface INavigationService : INotifyPropertyChanged
{
    object? CurrentView { get; }
    object NavigateTo<TViewModel>() where TViewModel : class;
    object NavigateTo<TViewModel>(object parameter) where TViewModel : class;
    void GoBack();
}
