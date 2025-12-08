using Domain.Users.ValueObjects;
using SharedKernel;

namespace Domain.Permissions;

/// <summary>
/// Связь между ролью и разрешением
/// </summary>
public sealed class RolePermission : DomainEventEntity
{
    public Guid RolePermissionId { get; private set; }
    public UserRole Role { get; private set; }
    public Permission Permission { get; private set; }

    private RolePermission() { } // For EF Core

    private RolePermission(UserRole role, Permission permission)
    {
        RolePermissionId = Guid.NewGuid();
        Role = role;
        Permission = permission;
    }

    public static RolePermission Create(UserRole role, Permission permission)
    {
        return new RolePermission(role, permission);
    }
}

