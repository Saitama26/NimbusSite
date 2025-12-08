using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.AccessPermissions.Queries.GetPermissions;

internal sealed class GetPermissionsQueryHandler : IQueryHandler<GetPermissionsQuery, IReadOnlyList<PermissionResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetPermissionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<PermissionResponse>>> Handle(GetPermissionsQuery query, CancellationToken cancellationToken)
    {
        var permissionsQuery = _context.AccessPermissions.AsQueryable();

        if (query.UserId.HasValue)
        {
            permissionsQuery = permissionsQuery.Where(p => p.UserId == query.UserId.Value);
        }

        if (query.ProjectId.HasValue)
        {
            permissionsQuery = permissionsQuery.Where(p => p.ProjectId == query.ProjectId.Value);
        }

        if (query.TenantId.HasValue)
        {
            permissionsQuery = permissionsQuery.Where(p => p.TenantId == query.TenantId.Value);
        }

        if (!query.IncludeRevoked)
        {
            permissionsQuery = permissionsQuery.Where(p => p.RevokedAt == default(DateTime));
        }

        var permissions = await permissionsQuery.ToListAsync(cancellationToken);

        var response = permissions.Select(permission => new PermissionResponse(
            permission.Id,
            permission.TenantId,
            permission.UserId,
            permission.ProjectId,
            permission.Role,
            permission.CreatedAt,
            permission.RevokedAt == default(DateTime) ? null : permission.RevokedAt
        )).ToList();

        return response;
    }
}

