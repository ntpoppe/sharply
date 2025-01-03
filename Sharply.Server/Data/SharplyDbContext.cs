using Microsoft.EntityFrameworkCore;
using Sharply.Server.Models;

namespace Sharply.Server.Data;

/// <summary>
/// Represents the database context for the Sharply application.
/// </summary>
public class SharplyDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<Message> Messages { get; set; }

    public DbSet<Models.Server> Servers { get; set; }

    public DbSet<UserServer> UserServers { get; set; }

    public DbSet<Channel> Channels { get; set; }

    public DbSet<UserChannel> UserChannels { get; set; }

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
        ConfigureMessages(modelBuilder);
        ConfigureServersAndChannels(modelBuilder);
        ConfigureUserChannelRelationship(modelBuilder);
        ConfigureUserServerRelationship(modelBuilder);
        SeedData(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private void ConfigureMessages(ModelBuilder modelBuilder)
    {
        // A message belongs to one user, and a user can have many messages
        modelBuilder.Entity<Message>()
            .HasOne(m => m.User)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.UserId);

        // A message belongs to one channel, and a channel can have many messages
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Channel)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChannelId)
            .OnDelete(DeleteBehavior.Cascade); // Deleting a channel deletes its messages
    }

    private void ConfigureServersAndChannels(ModelBuilder modelBuilder)
    {
        // A server can have many channels, and a channel belongs to one server
        modelBuilder.Entity<Models.Server>()
            .HasMany(s => s.Channels)
            .WithOne(c => c.Server)
            .HasForeignKey(c => c.ServerId)
            .OnDelete(DeleteBehavior.Cascade);

        // A channel can have many messages, and a message belongs to one channel
        modelBuilder.Entity<Channel>()
            .HasMany(c => c.Messages)
            .WithOne(m => m.Channel)
            .HasForeignKey(m => m.ChannelId);
    }

    private void ConfigureUserChannelRelationship(ModelBuilder modelBuilder)
    {
        // Composite key for UserChannel (UserId and ChannelId together make the primary key)
        modelBuilder.Entity<UserChannel>()
            .HasKey(uc => new { uc.UserId, uc.ChannelId });

        // A user can join many channels, and a channel can have many users (many-to-many)
        modelBuilder.Entity<UserChannel>()
            .HasOne(uc => uc.User)
            .WithMany(u => u.UserChannels)
            .HasForeignKey(uc => uc.UserId);

        modelBuilder.Entity<UserChannel>()
            .HasOne(uc => uc.Channel)
            .WithMany(c => c.UserChannels)
            .HasForeignKey(uc => uc.ChannelId);

        modelBuilder.Entity<UserChannel>()
            .Property(uc => uc.IsActive)
            .HasDefaultValue(1);
    }

    private void ConfigureUserServerRelationship(ModelBuilder modelBuilder)
    {
        // Composite key for UserServer (UserId and ServerId together make the primary key)
        modelBuilder.Entity<UserServer>()
            .HasKey(us => new { us.UserId, us.ServerId });

        // A user can join many servers, and a server can have many users (many-to-many)
        modelBuilder.Entity<UserServer>()
            .HasOne(us => us.User)
            .WithMany(u => u.UserServers)
            .HasForeignKey(us => us.UserId);

        modelBuilder.Entity<UserServer>()
            .HasOne(us => us.Server)
            .WithMany(s => s.UserServers)
            .HasForeignKey(us => us.ServerId);

        modelBuilder.Entity<UserServer>()
            .Property(us => us.IsActive)
            .HasDefaultValue(1);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed a default server
        modelBuilder.Entity<Models.Server>().HasData(new Models.Server { Id = 1, Name = "Global", OwnerId = 1 });

        // Seed a default channel in the default server
        modelBuilder.Entity<Channel>().HasData(new Channel { Id = 1, Name = "/general", ServerId = 1 });
        modelBuilder.Entity<Channel>().HasData(new Channel { Id = 2, Name = "/testing", ServerId = 1 });
    }
}
