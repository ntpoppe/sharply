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
	/// Gets or sets the <see cref"DbSet{TEntity}"/> for messages.
	/// </summary>
	public required DbSet<Message> Messages { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TEntity}"/> for servers.
    /// </summary>
    public required DbSet<Models.Server> Servers { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TEntity}"> for channels.
    /// </summary>
    public required DbSet<Channel> Channels { get; set; }

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

		modelBuilder.Entity<Models.Server>()
			.HasMany(s => s.Channels)
			.WithOne(c => c.Server)
			.HasForeignKey(c => c.ServerId);

        modelBuilder.Entity<Models.Server>().HasData(new Models.Server { Id = 1, Name = "Global" });
        modelBuilder.Entity<Channel>().HasData(new Channel { Id = 1, Name = "General", ServerId = 1 });

        base.OnModelCreating(modelBuilder);
	}
}
