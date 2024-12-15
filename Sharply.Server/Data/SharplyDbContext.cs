using Microsoft.EntityFrameworkCore;
using Sharply.Server.Models;

namespace Sharply.Server.Data;

/// <summary>
/// Represents the database context for the Sharply application.
/// </summary>
public class SharplyDbContext : DbContext
{
    public required DbSet<User> Users { get; set; }

    public required DbSet<Message> Messages { get; set; }

    public required DbSet<Models.Server> Servers { get; set; }

    public required DbSet<UserServer> UserServers { get; set; }

    public required DbSet<Channel> Channels { get; set; }

    public required DbSet<UserChannel> UserChannels { get; set; }

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
        modelBuilder.Entity<Message>()
            .HasOne(m => m.User) // A message has one user
            .WithMany(u => u.Messages) // A user has many messages
            .HasForeignKey(m => m.UserId); // Foreign key property in Message

        modelBuilder.Entity<Models.Server>()
            .HasMany(s => s.Channels)
            .WithOne(c => c.Server)
            .HasForeignKey(c => c.ServerId);

        modelBuilder.Entity<UserChannel>()
           .HasKey(uc => new { uc.UserId, uc.ChannelId });

        modelBuilder.Entity<UserChannel>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.UserChannels)
            .HasForeignKey(uc => uc.UserId);

        modelBuilder.Entity<UserChannel>()
            .HasOne(uc => uc.Channel)
            .WithMany(c => c.UserChannels)
            .HasForeignKey(uc => uc.ChannelId);

        // Configure composite primary key
        modelBuilder.Entity<UserServer>()
            .HasKey(us => new { us.UserId, us.ServerId });

        // Configure relationships
        modelBuilder.Entity<UserServer>()
            .HasOne(us => us.User)
            .WithMany(u => u.UserServers)
            .HasForeignKey(us => us.UserId);

        modelBuilder.Entity<UserServer>()
            .HasOne(us => us.Server)
            .WithMany(s => s.UserServers)
            .HasForeignKey(us => us.ServerId);


        modelBuilder.Entity<Models.Server>().HasData(new Models.Server { Id = 1, Name = "Global" });
        modelBuilder.Entity<Channel>().HasData(new Channel { Id = 1, Name = "General", ServerId = 1 });

        base.OnModelCreating(modelBuilder);
    }
}
