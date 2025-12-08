using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Access;
using Domain.Auth;
using Domain.Permissions;
using Domain.Projects;
using Domain.Tasks;
using Domain.Tenants;
using Domain.Users;
using Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Infrastructure.Data;

public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly ILogger<ApplicationDbContext> _logger;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDomainEventDispatcher domainEventDispatcher,
        ILogger<ApplicationDbContext> logger)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
        _logger = logger;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectTask> Tasks { get; set; }
    public DbSet<AccessPermission> AccessPermissions { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Собираем все доменные события из измененных сущностей
        var domainEvents = GetDomainEvents();

        _logger?.LogDebug(
            "Saving changes to database. Found {EventCount} domain event(s) to publish",
            domainEvents.Count);

        // Сохраняем изменения в БД
        var result = await base.SaveChangesAsync(cancellationToken);

        // Публикуем события после успешного сохранения (только если dispatcher доступен)
        if (domainEvents.Count > 0 && _domainEventDispatcher != null)
        {
            _logger?.LogInformation(
                "Publishing {EventCount} domain event(s) after successful save",
                domainEvents.Count);

            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
            
            // Очищаем события после публикации
            ClearDomainEvents();
        }

        return result;
    }

    private List<IDomainEvent> GetDomainEvents()
    {
        var domainEvents = new List<IDomainEvent>();

        var entitiesWithEvents = ChangeTracker
            .Entries<IDomainEventPublisher>()
            .Where(e => e.Entity.Events.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            domainEvents.AddRange(entity.Events);
        }

        return domainEvents;
    }

    private void ClearDomainEvents()
    {
        var entitiesWithEvents = ChangeTracker
            .Entries<IDomainEventPublisher>()
            .Where(e => e.Entity.Events.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }
    }
}