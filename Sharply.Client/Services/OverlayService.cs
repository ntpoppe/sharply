using Sharply.Client.Interfaces;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Sharply.Client.Services;

public class OverlayService : IOverlayService
{
    private readonly IServiceProvider _serviceProvider;

    public event PropertyChangedEventHandler? PropertyChanged;

    public OverlayService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _isOverlayVisible = false;
    }

    public bool IsOverlayVisible
    {
        get => _isOverlayVisible;
        private set
        {
            _isOverlayVisible = value;
            OnPropertyChanged();
        }
    }
    private bool _isOverlayVisible;

    public object? CurrentOverlayView
    {
        get => _currentOverlayView;
        set
        {
            _currentOverlayView = value;
            OnPropertyChanged();
        }
    }
    private object? _currentOverlayView;

    public void ShowOverlay<TViewModel>() where TViewModel : class, IOverlay
    {
        HideOverlay();
        var view = _serviceProvider.GetService(typeof(TViewModel));
        if (view != null)
        {
            CurrentOverlayView = view;
            IsOverlayVisible = true;
        }
		else
        {
            throw new InvalidOperationException($"Overlay ViewModel of type {typeof(TViewModel).Name} is not registered.");
        }
    }

    public void HideOverlay()
    {
        CurrentOverlayView = null;
        IsOverlayVisible = false;
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
