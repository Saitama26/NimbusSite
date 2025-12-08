using Application.Abstractions.Auth;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Authentication.Commands.Logout;

internal sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand>
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IAuthenticationService authenticationService,
        ILogger<LogoutCommandHandler> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Attempting logout");

        var result = await _authenticationService.LogoutAsync(
            command.RefreshToken,
            cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "User logged out successfully");
        }
        else
        {
            _logger.LogWarning(
                "Logout failed: {Error}",
                result.Error?.Description);
        }

        return result;
    }
}

