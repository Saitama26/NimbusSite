using Application.Abstractions.Repositories;
using Application.Abstractions.Messaging;
using Domain.Users;
using SharedKernel;

namespace Application.Users.Queries.GetUsers;

internal sealed class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, IReadOnlyList<UserResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<IReadOnlyList<UserResponse>>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<User> users;

        if (query.TenantId.HasValue)
        {
            users = await _userRepository.GetByTenantIdAsync(query.TenantId.Value, cancellationToken);
        }
        else
        {
            // Если нет TenantId, нужно добавить метод GetAllAsync в репозиторий
            // Пока возвращаем пустой список или можно добавить метод в репозиторий
            users = Array.Empty<User>();
        }

        var response = users.Select(user => new UserResponse(
            user.Id,
            user.TenantId,
            user.UserName,
            user.Email,
            user.Role,
            user.IsActive,
            user.CreatedAt
        )).ToList();

        return response;
    }
}

