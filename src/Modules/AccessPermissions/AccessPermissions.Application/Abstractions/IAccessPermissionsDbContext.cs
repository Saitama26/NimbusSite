using Microsoft.EntityFrameworkCore;
using DomainAccessPermission = AccessPermissions.Domain.Entities.AccessPermission;

namespace AccessPermissions.Application.Abstractions;

/// <summary>
/// Интерфейс для доступа к DbContext без зависимости от Infrastructure
/// </summary>
public interface IAccessPermissionsDbContext
{
    DbSet<DomainAccessPermission> AccessPermissions { get; }

    System.Threading.Tasks.Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

