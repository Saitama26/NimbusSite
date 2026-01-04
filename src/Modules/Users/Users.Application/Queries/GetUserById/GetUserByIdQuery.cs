using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Users.Application.Abstractions;
using Users.Contracts.Api.Responses;
using Users.Contracts.Enums;
using Users.Domain.Errors;

namespace Users.Application.Queries.GetUserById;

/// <summary>
/// Запрос получения пользователя по ID
/// </summary>
public sealed record GetUserByIdQuery(Guid UserId, int TenantId) : IQuery<UserResponse>;

/// <summary>
/// Обработчик запроса получения пользователя по ID
/// </summary>
internal sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    private readonly IUsersDbContext _dbContext;

    public GetUserByIdQueryHandler(IUsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        // Найти пользователя с проверкой TenantId, исключая удаленных
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == query.UserId && u.TenantId == query.TenantId && u.Status != Users.Domain.Enums.UserStatus.Deleted, cancellationToken);
        if (user == null)
        {
            return Result<UserResponse>.Failure(UserErrors.NotFound(query.UserId));
        }

        // Маппинг в Response
        var response = new UserResponse(
            user.Id,
            user.Email,
            user.Name,
            (Users.Contracts.Enums.UserStatusContract)(int)user.Status,
            user.CreatedAt,
            user.UpdatedAt,
            user.LastLoginAt,
            user.Phone,
            user.Bio);

        return Result<UserResponse>.Success(response);
    }
}

