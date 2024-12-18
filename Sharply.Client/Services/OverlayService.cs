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

    public void ShowOverlay<TView>() where TView : class
    {
        var view = _serviceProvider.GetService(typeof(TView));
        if (view != null)
        {
            CurrentOverlayView = view;
            IsOverlayVisible = true;
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
