using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using System;
using System.Collections.Specialized;

namespace Sharply.Client.Behaviors;

public class ScrollToEndBehavior : Behavior<ScrollViewer>
{
    public static readonly StyledProperty<INotifyCollectionChanged?> ItemsSourceProperty =
             AvaloniaProperty.Register<ScrollToEndBehavior, INotifyCollectionChanged?>(nameof(ItemsSource));

    public INotifyCollectionChanged? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null)
            AssociatedObject.ScrollToEnd();

        // Keep track of the current ItemsSource so we can unsubscribe later.
        this.GetObservable(ItemsSourceProperty).Subscribe(newValue =>
        {
            if (ItemsSource != null)
            {
                UnsubscribeFromItemsSource(ItemsSource);
            }

            if (newValue != null)
            {
                SubscribeToItemsSource(newValue);
                AssociatedObject?.ScrollToEnd();
            }

            ItemsSource = newValue;
        });

        if (ItemsSource != null)
        {
            SubscribeToItemsSource(ItemsSource);
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (ItemsSource != null)
        {
            ItemsSource.CollectionChanged -= OnCollectionChanged;
        }
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && AssociatedObject != null)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                AssociatedObject.ScrollToEnd();
            });
        }
    }

    private void SubscribeToItemsSource(INotifyCollectionChanged itemsSource)
    {
        itemsSource.CollectionChanged += OnCollectionChanged;
    }

    private void UnsubscribeFromItemsSource(INotifyCollectionChanged itemsSource)
    {
        itemsSource.CollectionChanged -= OnCollectionChanged;
    }
}
