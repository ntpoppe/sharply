namespace Sharply.Client.Interfaces;

/// <summary>
/// Flags a view model as navigable. 
/// </summary>
public interface INavigable
{
    void OnNavigatedTo(object? parameter);
}
