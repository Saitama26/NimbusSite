using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Users.Application.Abstractions;
using Users.Contracts.Api.Responses;
using Users.Contracts.Enums;

namespace Users.Application.Queries.GetUsers;

/// <summary>
/// Запрос получения списка пользователей
/// </summary>
public sealed record GetUsersQuery(int TenantId) : IQuery<IEnumerable<UserListResponse>>;

/// <summary>
/// Обработчик запроса получения списка пользователей
/// </summary>
internal sealed class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, IEnumerable<UserListResponse>>
{
    private readonly IUsersDbContext _dbContext;

    public GetUsersQueryHandler(IUsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<UserListResponse>>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        var usersQuery = _dbContext.Users
            .AsNoTracking()
            .Where(u => u.TenantId == query.TenantId && u.Status != Users.Domain.Enums.UserStatus.Deleted);

        var users = await usersQuery
            .OrderBy(u => u.Name)
            .Select(u => new UserListResponse(
                u.Id,
                u.Email,
                u.Name,
                (UserStatusContract)(int)u.Status,
                u.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<UserListResponse>>.Success(users);
    }
}

