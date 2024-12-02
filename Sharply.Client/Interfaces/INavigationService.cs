using System.ComponentModel;

namespace Sharply.Client.Interfaces;

public interface INavigationService : INotifyPropertyChanged
{
    object? CurrentView { get; }
    void NavigateTo<TViewModel>() where TViewModel : class;
    void NavigateTo<TViewModel>(object parameter) where TViewModel : class;
    void GoBack();
}
