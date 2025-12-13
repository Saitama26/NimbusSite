using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using AccessPermissions.Application.Abstractions;
using AccessPermissions.Application.DTOs;
using AccessPermissions.Domain.Errors;

namespace AccessPermissions.Application.Queries.GetAccessPermissionById;

/// <summary>
/// Запрос получения разрешения доступа по ID
/// </summary>
public sealed record GetAccessPermissionByIdQuery(Guid PermissionId) : IQuery<AccessPermissionDto>;

/// <summary>
/// Обработчик запроса получения разрешения доступа по ID
/// </summary>
internal sealed class GetAccessPermissionByIdQueryHandler : IQueryHandler<GetAccessPermissionByIdQuery, AccessPermissionDto>
{
    private readonly IAccessPermissionRepository _repository;

    public GetAccessPermissionByIdQueryHandler(IAccessPermissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AccessPermissionDto>> Handle(GetAccessPermissionByIdQuery query, CancellationToken cancellationToken)
    {
        var permission = await _repository.GetByIdAsync(query.PermissionId, cancellationToken);
        if (permission == null)
        {
            return Result<AccessPermissionDto>.Failure(AccessPermissionErrors.NotFound(query.PermissionId));
        }

        var dto = new AccessPermissionDto(
            permission.Id,
            permission.TenantId,
            permission.UserId,
            permission.ProjectId,
            permission.TaskId,
            permission.Scope,
            permission.Action,
            permission.Type,
            permission.CreatedByUserId,
            permission.CreatedAt,
            permission.ExpiresAt,
            permission.Note,
            permission.UpdatedAt,
            permission.IsValid);

        return Result<AccessPermissionDto>.Success(dto);
    }
}

