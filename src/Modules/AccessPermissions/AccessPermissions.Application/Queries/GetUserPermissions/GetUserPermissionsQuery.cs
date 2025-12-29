using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AccessPermissions.Application.Abstractions;
using AccessPermissions.Contracts.Api.Responses;
using AccessPermissions.Contracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace AccessPermissions.Application.Queries.GetUserPermissions;

/// <summary>
/// Запрос получения разрешений пользователя
/// </summary>
public sealed record GetUserPermissionsQuery(
    Guid UserId,
    int TenantId,
    Guid? ProjectId = null) : IQuery<IEnumerable<AccessPermissionResponse>>;

/// <summary>
/// Обработчик запроса получения разрешений пользователя
/// </summary>
internal sealed class GetUserPermissionsQueryHandler : IQueryHandler<GetUserPermissionsQuery, IEnumerable<AccessPermissionResponse>>
{
    private readonly IAccessPermissionsDbContext _dbContext;

    public GetUserPermissionsQueryHandler(IAccessPermissionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<AccessPermissionResponse>>> Handle(GetUserPermissionsQuery query, CancellationToken cancellationToken)
    {
        var permissionsQuery = _dbContext.AccessPermissions
            .AsNoTracking()
            .Where(p => p.UserId == query.UserId && p.TenantId == query.TenantId && p.IsValid);

        if (query.ProjectId.HasValue)
        {
            permissionsQuery = permissionsQuery.Where(p => p.ProjectId == query.ProjectId.Value);
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

