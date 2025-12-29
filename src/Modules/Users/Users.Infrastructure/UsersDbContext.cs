using Microsoft.EntityFrameworkCore;
using Users.Application.Abstractions;
using Users.Domain.Entities;
using Users.Infrastructure.Persistence.Configurations;

namespace Users.Infrastructure;

/// <summary>
/// DbContext для Users
/// Работает с tenant-специфичной БД (таблицы в корне базы данных без схем)
/// Connection string определяется динамически через ShardResolver по TenantId
/// </summary>
public class UsersDbContext : DbContext, IUsersDbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Применяем конфигурации
        modelBuilder.ApplyConfiguration(new UserConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}

