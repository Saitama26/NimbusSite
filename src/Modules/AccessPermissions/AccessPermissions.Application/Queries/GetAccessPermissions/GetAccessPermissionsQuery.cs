using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AccessPermissions.Application.Abstractions;
using AccessPermissions.Contracts.Api.Responses;
using AccessPermissions.Contracts.Enums;
using AccessPermissions.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AccessPermissions.Application.Queries.GetAccessPermissions;

/// <summary>
/// Запрос получения списка разрешений доступа
/// </summary>
public sealed record GetAccessPermissionsQuery(
    int TenantId,
    Guid? UserId = null,
    Guid? ProjectId = null,
    Guid? TaskId = null,
    PermissionScope? Scope = null,
    PermissionAction? Action = null,
    PermissionType? Type = null) : IQuery<IEnumerable<AccessPermissionResponse>>;

/// <summary>
/// Обработчик запроса получения списка разрешений доступа
/// </summary>
internal sealed class GetAccessPermissionsQueryHandler : IQueryHandler<GetAccessPermissionsQuery, IEnumerable<AccessPermissionResponse>>
{
    private readonly IAccessPermissionsDbContext _dbContext;

    public GetAccessPermissionsQueryHandler(IAccessPermissionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<AccessPermissionResponse>>> Handle(GetAccessPermissionsQuery query, CancellationToken cancellationToken)
    {
        var permissionsQuery = _dbContext.AccessPermissions
            .AsNoTracking()
            .Where(p => p.TenantId == query.TenantId);

        if (query.UserId.HasValue)
        {
            permissionsQuery = permissionsQuery.Where(p => p.UserId == query.UserId.Value);
        }

        if (query.ProjectId.HasValue)
        {
            permissionsQuery = permissionsQuery.Where(p => p.ProjectId == query.ProjectId.Value);
        }

        if (query.TaskId.HasValue)
        {
            permissionsQuery = permissionsQuery.Where(p => p.TaskId == query.TaskId.Value);
        }

        if (query.Scope.HasValue)
        {
            permissionsQuery = permissionsQuery.Where(p => p.Scope == query.Scope.Value);
        }

        if (query.Action.HasValue)
        {
            permissionsQuery = permissionsQuery.Where(p => p.Action == query.Action.Value);
        }

        if (query.Type.HasValue)
        {
            permissionsQuery = permissionsQuery.Where(p => p.Type == query.Type.Value);
        }

        var permissions = await permissionsQuery
            .Select(p => new AccessPermissionResponse(
                p.Id,
                p.TenantId,
                p.UserId,
                p.ProjectId,
                p.TaskId,
                (PermissionScopeContract)(int)p.Scope,
                (PermissionActionContract)(int)p.Action,
                (PermissionTypeContract)(int)p.Type,
                p.CreatedByUserId,
                p.CreatedAt,
                p.ExpiresAt,
                p.Note,
                p.UpdatedAt,
                p.IsValid))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<AccessPermissionResponse>>.Success(permissions);
    }
}

