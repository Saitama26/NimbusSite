using Application.Abstractions.Auth;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Authentication.Commands.RefreshToken;

internal sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IAuthenticationService authenticationService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Attempting to refresh token");

        var authResult = await _authenticationService.RefreshTokenAsync(
            command.RefreshToken,
            cancellationToken);

        if (authResult.IsSuccess == false)
        {
            _logger.LogWarning(
                "Token refresh failed: {Error}",
                authResult.Error?.Description);
            return authResult.Error!;
        }

        _logger.LogInformation(
            "Token refreshed successfully for user {UserId}",
            authResult.Value!.User.Id);

        return new RefreshTokenResponse(
            authResult.Value.AccessToken,
            authResult.Value.RefreshToken,
            authResult.Value.ExpiresAt);
    }
}

