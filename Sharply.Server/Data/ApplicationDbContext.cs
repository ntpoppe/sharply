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
    public required DbSet<User> Users { get; set; }

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

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		// Relationship between Message and User
		modelBuilder.Entity<Message>()
			.HasOne(m => m.User) // A message has one user
			.WithMany(u => u.Messages) // A user has many messages
			.HasForeignKey(m => m.Username) // Foreign key property in Message
			.HasPrincipalKey(u => u.Username); // Principal key in User

		base.OnModelCreating(modelBuilder);
	}

}
