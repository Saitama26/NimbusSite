using Microsoft.EntityFrameworkCore;
using Domain.Users;
using Domain.Projects;
using Domain.Tasks;
using Domain.Access;
using Domain.Tenants;
using Domain.Permissions;
using Domain.Auth;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    // DbSets for aggregates
    DbSet<User> Users { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectTask> Tasks { get; }
    DbSet<AccessPermission> AccessPermissions { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    // Saving changes
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}