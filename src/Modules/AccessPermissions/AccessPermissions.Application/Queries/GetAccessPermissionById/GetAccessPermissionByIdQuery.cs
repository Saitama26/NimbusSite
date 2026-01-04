using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AccessPermissions.Application.Abstractions;
using AccessPermissions.Contracts.Api.Responses;
using AccessPermissions.Contracts.Enums;
using AccessPermissions.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace AccessPermissions.Application.Queries.GetAccessPermissionById;

/// <summary>
/// Запрос получения разрешения доступа по ID
/// </summary>
public sealed record GetAccessPermissionByIdQuery(
    Guid PermissionId,
    int TenantId) : IQuery<AccessPermissionResponse>;

/// <summary>
/// Обработчик запроса получения разрешения доступа по ID
/// </summary>
internal sealed class GetAccessPermissionByIdQueryHandler : IQueryHandler<GetAccessPermissionByIdQuery, AccessPermissionResponse>
{
    private readonly IAccessPermissionsDbContext _dbContext;

    public GetAccessPermissionByIdQueryHandler(IAccessPermissionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<AccessPermissionResponse>> Handle(GetAccessPermissionByIdQuery query, CancellationToken cancellationToken)
    {
        var permission = await _dbContext.AccessPermissions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.PermissionId && p.TenantId == query.TenantId, cancellationToken);
        if (permission == null)
        {
            return Result<AccessPermissionResponse>.Failure(AccessPermissionErrors.NotFound(query.PermissionId));
        }

        var response = new AccessPermissionResponse(
            permission.Id,
            permission.TenantId,
            permission.UserId,
            permission.ProjectId,
            permission.TaskId,
            (PermissionScopeContract)(int)permission.Scope,
            (PermissionActionContract)(int)permission.Action,
            (PermissionTypeContract)(int)permission.Type,
            permission.CreatedByUserId,
            permission.CreatedAt,
            permission.ExpiresAt,
            permission.Note,
            permission.UpdatedAt,
            permission.IsValid);

        return Result<AccessPermissionResponse>.Success(response);
    }
}

