using Microsoft.EntityFrameworkCore;
using Sharply.Server.Models;

namespace Sharply.Server.Data;

/// <summary>
/// Represents the database context for the Sharply application.
/// </summary>
public class SharplyDbContext : DbContext
{
    /// <summary>
    /// Gets or sets the <see cref="DbSet{TEntity}"/> for users.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SharplyDbContext"/> class.
    /// </summary>
    /// <param name="options">
    /// The options to configure the <see cref="SharplyDbContext"/>.
    /// Includes connection strings and database provider details.
    /// </param>
    public SharplyDbContext(DbContextOptions<SharplyDbContext> options)
        : base(options)
    {
    }
}
