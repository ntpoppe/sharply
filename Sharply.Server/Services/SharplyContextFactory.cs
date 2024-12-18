using Microsoft.EntityFrameworkCore;
using Sharply.Server.Interfaces;

namespace Sharply.Server.Services;

/// <summary>
/// Manages operations specific to channels, such as retrieving messages, adding users to channels, or modifying channel properties.
/// </summary>
public class SharplyContextFactory<T> : ISharplyContextFactory<T> where T : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SharplyContextFactory(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public T CreateSharplyContext()
    {
        var scope = _scopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }
}


