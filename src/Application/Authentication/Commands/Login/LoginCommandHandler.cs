using Application.Abstractions.Auth;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Authentication.Commands.Login;

internal sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IAuthenticationService authenticationService,
        ILogger<LoginCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Attempting login for email '{Email}'",
            command.Email);

        var authResult = await _authenticationService.AuthenticateAsync(
            command.Email,
            command.Password,
            cancellationToken);

        if (authResult.IsSuccess == false)
        {
            _logger.LogWarning(
                "Login failed for email '{Email}': {Error}",
                command.Email,
                authResult.Error?.Description);
            return authResult.Error!;
        }

        var user = authResult.Value!.User;

        _logger.LogInformation(
            "User {UserId} ({Email}) logged in successfully",
            user.Id,
            user.Email);

        return new LoginResponse(
            user.Id,
            user.UserName,
            user.Email,
            authResult.Value.AccessToken,
            authResult.Value.RefreshToken,
            authResult.Value.ExpiresAt);
    }
}

