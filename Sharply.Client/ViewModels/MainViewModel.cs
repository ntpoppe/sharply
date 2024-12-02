using Sharply.Client.Interfaces;

namespace Sharply.Client.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly INavigationService NavigationService;

    #region Constructors

    public MainViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;

        // Start with the LoginViewModel
        NavigationService.NavigateTo<LoginViewModel>();

        // Listen for changes in CurrentView
        NavigationService.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(NavigationService.CurrentView))
            {
                OnPropertyChanged(nameof(CurrentView));
            }
        };
    }

    #endregion

    #region Properties


    public string Title { get; } = "sharply";

    public object? CurrentView => NavigationService.CurrentView;
    #endregion
}
