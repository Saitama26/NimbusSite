using Application.Abstractions.Messaging;

namespace Application.Users.Queries.GetUsers;

public sealed record GetUsersQuery(
    Guid? TenantId = null
) : IQuery<IReadOnlyList<UserResponse>> { }

