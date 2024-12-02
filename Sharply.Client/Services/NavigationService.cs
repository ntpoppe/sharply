using Sharply.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sharply.Client.Services;
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<object> _navigationStack = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object? CurrentView
    {
        get => _currentView;
        private set
        {
            _currentView = value;
            OnPropertyChanged();
        }
    }
    private object? _currentView;

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        var viewModel = _serviceProvider.GetService(typeof(TViewModel));
        if (viewModel != null)
        {
            _navigationStack.Push(viewModel);
            CurrentView = viewModel;
        }
    }

    public void NavigateTo<TViewModel>(object parameter) where TViewModel : class
    {
        var viewModel = _serviceProvider.GetService(typeof(TViewModel));
        if (viewModel != null && viewModel is INavigable navigable)
        {
            navigable.OnNavigatedTo(parameter);
            _navigationStack.Push(viewModel);
            CurrentView = viewModel;
        }
    }

    public void GoBack()
    {
        if (_navigationStack.Count > 1)
        {
            _navigationStack.Pop();
            CurrentView = _navigationStack.Peek();
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
