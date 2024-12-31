using System.ComponentModel;

namespace Sharply.Client.Interfaces;

public interface IOverlayService : INotifyPropertyChanged
{
    bool IsOverlayVisible { get; }
    object? CurrentOverlayView { get; }

    void ShowOverlay<TView>() where TView : class, IOverlay;
    void HideOverlay();
}
