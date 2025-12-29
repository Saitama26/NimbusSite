using Microsoft.EntityFrameworkCore;
using Identity.Application.Abstractions;
using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence.Configurations;
using Identity.Infrastructure.Views.TenantsViews;
using Identity.Infrastructure.Views.UsersViews;

namespace Identity.Infrastructure;

/// <summary>
/// DbContext для Identity
/// Работает с tenant-специфичной БД (таблицы в корне базы данных без схем)
/// </summary>
public class IdentityDbContext : DbContext, IIdentityDbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    public DbSet<UserCredentials> UserCredentials => Set<UserCredentials>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<UserView> UserViews => Set<UserView>();
    public DbSet<TenantView> TenantViews => Set<TenantView>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Применяем конфигурации
        modelBuilder.ApplyConfiguration(new UserCredentialsConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new SessionConfiguration());
        modelBuilder.ApplyConfiguration(new UserViewConfiguration());
        modelBuilder.ApplyConfiguration(new TenantViewConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}

