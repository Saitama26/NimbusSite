using Application.Abstractions.Messaging;
using Application.Abstractions.Repositories;
using Domain.Users.Errors;
using SharedKernel;

namespace Application.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.id, cancellationToken);

        if(user is null) 
        {
            return UserErrors.NotFound(request.id);
        }

        return new UserResponse(
            UserId: user.Id,
            TenantId: user.TenantId,
            UserName: user.UserName,
            Email: user.Email,
            Role: user.Role);
    }
}