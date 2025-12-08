using Domain.Users.ValueObjects;

namespace Domain.Permissions;

/// <summary>
/// Конфигурация разрешений для ролей
/// Можно использовать вместо БД для простых случаев
/// </summary>
public static class PermissionConfiguration
{
    /// <summary>
    /// Получить все разрешения для роли
    /// </summary>
    public static IReadOnlyList<Permission> GetPermissionsForRole(UserRole role)
    {
        return role switch
        {
            UserRole.Admin => GetAllPermissions(),
            UserRole.Manager => GetManagerPermissions(),
            UserRole.Participant => GetParticipantPermissions(),
            UserRole.Observer => GetObserverPermissions(),
            _ => Array.Empty<Permission>()
        };
    }

    private static IReadOnlyList<Permission> GetAllPermissions()
    {
        return Enum.GetValues<Permission>().ToList();
    }

    private static IReadOnlyList<Permission> GetManagerPermissions()
    {
        return new List<Permission>
        {
            // Users
            Permission.UsersView,
            
            // Projects
            Permission.ProjectsView,
            Permission.ProjectsCreate,
            Permission.ProjectsUpdate,
            Permission.ProjectsClose,
            Permission.ProjectsManageMembers,
            
            // Tasks
            Permission.TasksView,
            Permission.TasksCreate,
            Permission.TasksUpdate,
            Permission.TasksAssign,
            Permission.TasksChangeStatus,            
        };
    }

    private static IReadOnlyList<Permission> GetParticipantPermissions()
    {
        return new List<Permission>
        {
            // Projects
            Permission.ProjectsView,
            
            // Tasks
            Permission.TasksView,
            Permission.TasksCreate,
            Permission.TasksUpdate,
            Permission.TasksChangeStatus
        };
    }

    private static IReadOnlyList<Permission> GetObserverPermissions()
    {
        return new List<Permission>
        {
            // Projects
            Permission.ProjectsView,
            
            // Tasks
            Permission.TasksView
        };
    }
}

