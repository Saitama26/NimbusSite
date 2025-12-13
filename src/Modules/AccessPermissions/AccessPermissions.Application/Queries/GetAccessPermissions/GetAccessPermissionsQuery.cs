using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AccessPermissions.Application.Abstractions;
using AccessPermissions.Application.DTOs;
using AccessPermissions.Domain.Enums;

namespace AccessPermissions.Application.Queries.GetAccessPermissions;

/// <summary>
/// Запрос получения списка разрешений доступа
/// </summary>
public sealed record GetAccessPermissionsQuery(
    Guid? TenantId = null,
    Guid? UserId = null,
    Guid? ProjectId = null,
    Guid? TaskId = null,
    PermissionScope? Scope = null,
    PermissionAction? Action = null,
    PermissionType? Type = null) : IQuery<IEnumerable<AccessPermissionDto>>;

/// <summary>
/// Обработчик запроса получения списка разрешений доступа
/// </summary>
internal sealed class GetAccessPermissionsQueryHandler : IQueryHandler<GetAccessPermissionsQuery, IEnumerable<AccessPermissionDto>>
{
    private readonly IAccessPermissionRepository _repository;

    public GetAccessPermissionsQueryHandler(IAccessPermissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<AccessPermissionDto>>> Handle(GetAccessPermissionsQuery query, CancellationToken cancellationToken)
    {
        var permissionsQuery = await _repository.GetAllAsync(cancellationToken);

        if (query.TenantId.HasValue)
        {
            permissionsQuery = permissionsQuery.Where(p => p.TenantId == query.TenantId.Value);
        }

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

        var permissions = await Task.FromResult(permissionsQuery.ToList());

        var dtos = permissions.Select(p => new AccessPermissionDto(
            p.Id,
            p.TenantId,
            p.UserId,
            p.ProjectId,
            p.TaskId,
            p.Scope,
            p.Action,
            p.Type,
            p.CreatedByUserId,
            p.CreatedAt,
            p.ExpiresAt,
            p.Note,
            p.UpdatedAt,
            p.IsValid));

        return Result<IEnumerable<AccessPermissionDto>>.Success(dtos);
    }
}

