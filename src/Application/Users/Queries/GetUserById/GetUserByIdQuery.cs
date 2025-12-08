using Application.Abstractions.Messaging;

namespace Application.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid id) : IQuery<UserResponse> { }