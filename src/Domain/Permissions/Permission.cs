namespace Domain.Permissions;

/// <summary>
/// Список всех разрешений в системе
/// </summary>
public enum Permission
{
    // Users
    UsersView = 100,
    UsersCreate = 101,
    UsersUpdate = 102,
    UsersDelete = 103,
    UsersManageRoles = 104,

    // Projects
    ProjectsView = 200,
    ProjectsCreate = 201,
    ProjectsUpdate = 202,
    ProjectsDelete = 203,
    ProjectsClose = 204,
    ProjectsArchive = 205,
    ProjectsManageMembers = 206,

    // Tasks
    TasksView = 300,
    TasksCreate = 301,
    TasksUpdate = 302,
    TasksDelete = 303,
    TasksAssign = 304,
    TasksChangeStatus = 305,

    // System
    SystemAdmin = 900,
    SystemSettings = 901
}