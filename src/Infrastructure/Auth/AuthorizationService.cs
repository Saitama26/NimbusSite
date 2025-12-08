using Application.Abstractions.Auth;
using Application.Abstractions.Data;
using Application.Abstractions.Repositories;
using Domain.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Auth;

internal sealed class AuthorizationService : IAuthorizationService
{
    private readonly IApplicationDbContext _context;
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<AuthorizationService> _logger;

    public AuthorizationService(
        IApplicationDbContext context,
        IProjectRepository projectRepository,
        ILogger<AuthorizationService> logger)
    {
        _context = context;
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, Permission permission, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found when checking permission", userId);
            return false;
        }

        // Получаем разрешения для роли пользователя
        var rolePermissions = await _context.RolePermissions
            .Where(rp => rp.Role == user.Role)
            .ToListAsync(cancellationToken);

        var hasPermission = rolePermissions.Any(rp => rp.Permission == permission);

        _logger.LogDebug(
            "Permission check for user {UserId}, permission {Permission}: {HasPermission}",
            userId,
            permission,
            hasPermission);

        return hasPermission;
    }

    public async Task<bool> HasProjectAccessAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);

        if (project is null)
        {
            return false;
        }

        // Проверяем, является ли пользователь владельцем
        if (project.OwnerId == userId)
        {
            return true;
        }

        // Проверяем, является ли пользователь участником проекта
        var isMember = await _context.Projects
            .Where(p => p.Id == projectId)
            .SelectMany(p => p.Members)
            .AnyAsync(m => m.Id == userId, cancellationToken);

        if (isMember)
        {
            return true;
        }

        // Проверяем через AccessPermission
        var hasAccessPermission = await _context.AccessPermissions
            .AnyAsync(
                ap => ap.UserId == userId 
                    && ap.ProjectId == projectId 
                    && ap.RevokedAt == default(DateTime),
                cancellationToken);

        return hasAccessPermission;
    }

    public async Task<bool> IsProjectOwnerAsync(Guid userId, Guid projectId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);

        if (project is null)
        {
            return false;
        }

        return project.OwnerId == userId;
    }

    public async Task<bool> HasTaskAccessAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task is null)
        {
            return false;
        }

        // Проверяем доступ через проект
        return await HasProjectAccessAsync(userId, task.ProjectId, cancellationToken);
    }
}

