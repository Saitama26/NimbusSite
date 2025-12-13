using Microsoft.EntityFrameworkCore;
using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence.Configurations;
using Identity.Infrastructure.Persistence.Sharding;

namespace Identity.Infrastructure;

/// <summary>
/// DbContext для Identity
/// </summary>
public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<UserCredentials> UserCredentials => Set<UserCredentials>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<ShardMapEntry> ShardMapEntries => Set<ShardMapEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserCredentialsConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new SessionConfiguration());
        modelBuilder.ApplyConfiguration(new ShardMapConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}

