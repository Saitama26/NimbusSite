using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AccessPermissions.Application.Abstractions;
using AccessPermissions.Application.DTOs;

namespace AccessPermissions.Application.Queries.GetUserPermissions;

/// <summary>
/// Запрос получения разрешений пользователя
/// </summary>
public sealed record GetUserPermissionsQuery(
    Guid UserId,
    Guid TenantId,
    Guid? ProjectId = null) : IQuery<IEnumerable<AccessPermissionDto>>;

/// <summary>
/// Обработчик запроса получения разрешений пользователя
/// </summary>
internal sealed class GetUserPermissionsQueryHandler : IQueryHandler<GetUserPermissionsQuery, IEnumerable<AccessPermissionDto>>
{
    private readonly IAccessPermissionRepository _repository;

    public GetUserPermissionsQueryHandler(IAccessPermissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<AccessPermissionDto>>> Handle(GetUserPermissionsQuery query, CancellationToken cancellationToken)
    {
        IQueryable<AccessPermissions.Domain.Entities.AccessPermission> permissionsQuery;

        if (query.ProjectId.HasValue)
        {
            permissionsQuery = await _repository.GetByUserIdAndProjectIdAsync(query.UserId, query.ProjectId.Value, cancellationToken);
        }
        else
        {
            permissionsQuery = await _repository.GetByUserIdAndTenantIdAsync(query.UserId, query.TenantId, cancellationToken);
        }

        // Фильтруем только действительные разрешения
        permissionsQuery = permissionsQuery.Where(p => p.IsValid);

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

