using Application.Abstractions.Auth;
using Application.Abstractions.Repositories;
using Domain.Users;
using Domain.Users.Errors;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Infrastructure.Auth;

internal sealed class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<AuthenticationService> _logger;
    private const int AccessTokenExpirationMinutes = 60;
    private const int RefreshTokenExpirationDays = 30;

    public AuthenticationService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenService refreshTokenService,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<Result<AuthenticationResult>> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Authenticating user with email '{Email}'", email);

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("User with email '{Email}' not found", email);
            return UserErrors.NotFoundByEmail(email);
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("User {UserId} is not active", user.Id);
            return UserErrors.Unauthorized();
        }

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid password for user '{Email}'", email);
            return UserErrors.InvalidCredentials();
        }

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes);

        await _refreshTokenService.SaveRefreshTokenAsync(
            user.Id,
            refreshToken,
            DateTime.UtcNow.AddDays(RefreshTokenExpirationDays),
            cancellationToken);

        _logger.LogInformation("User {UserId} authenticated successfully", user.Id);

        return new AuthenticationResult(
            user,
            accessToken,
            refreshToken,
            expiresAt);
    }

    public async Task<Result<AuthenticationResult>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refreshing token");

        var userId = await _refreshTokenService.GetUserIdByRefreshTokenAsync(refreshToken, cancellationToken);

        if (!userId.HasValue)
        {
            _logger.LogWarning("Invalid refresh token");
            return UserErrors.InvalidRefreshToken();
        }

        var isValid = await _refreshTokenService.IsRefreshTokenValidAsync(refreshToken, cancellationToken);
        if (!isValid)
        {
            _logger.LogWarning("Refresh token expired");
            return UserErrors.RefreshTokenExpired();
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found", userId.Value);
            return UserErrors.NotFound(userId.Value);
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("User {UserId} is not active", user.Id);
            return UserErrors.Unauthorized();
        }

        // Инвалидируем старый токен
        await _refreshTokenService.InvalidateRefreshTokenAsync(refreshToken, cancellationToken);

        // Генерируем новые токены
        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes);

        await _refreshTokenService.SaveRefreshTokenAsync(
            user.Id,
            newRefreshToken,
            DateTime.UtcNow.AddDays(RefreshTokenExpirationDays),
            cancellationToken);

        _logger.LogInformation("Token refreshed successfully for user {UserId}", user.Id);

        return new AuthenticationResult(
            user,
            newAccessToken,
            newRefreshToken,
            expiresAt);
    }

    public async Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Logging out user");

        var userId = await _refreshTokenService.GetUserIdByRefreshTokenAsync(refreshToken, cancellationToken);

        if (!userId.HasValue)
        {
            _logger.LogWarning("Invalid refresh token during logout");
            return UserErrors.InvalidRefreshToken();
        }

        await _refreshTokenService.InvalidateRefreshTokenAsync(refreshToken, cancellationToken);

        _logger.LogInformation("User {UserId} logged out successfully", userId.Value);

        return Result.Success();
    }
}

