using Microsoft.EntityFrameworkCore;

namespace Sharply.Server.Interfaces;
public interface ISharplyContextFactory<T> where T : DbContext
{
    T CreateSharplyContext();
}


