using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;
using Users.Infrastructure.Persistence.Configurations;
using Users.Infrastructure.Persistence.Sharding;

namespace Users.Infrastructure;

/// <summary>
/// DbContext для Users.
/// </summary>
public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ShardMapEntry> ShardMapEntries => Set<ShardMapEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ShardMapConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}

