using Domain.Users;

namespace Application.Abstractions.Auth;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? GetUserIdFromToken(string token);
    bool IsTokenValid(string token);
}

