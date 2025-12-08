using Application.Authentication.Commands.Login;
using Application.Authentication.Commands.Logout;
using Application.Authentication.Commands.RefreshToken;
using Application.Users.Commands.CreateUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Register a new user and return identifiers.
    /// </summary>
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Register new user",
        Description = "Creates a tenant user and returns the created user id."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "User created", typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation error", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Email already exists", typeof(Error))]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Login with email and password.
    /// </summary>
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Login",
        Description = "Returns access/refresh tokens for valid credentials."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Authenticated", typeof(LoginResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation error", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid credentials", typeof(Error))]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Refresh access token using refresh token.
    /// </summary>
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Refresh token",
        Description = "Exchanges a valid refresh token for a new access/refresh pair."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Refreshed", typeof(RefreshTokenResponse))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation error", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid/expired refresh token", typeof(Error))]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Logout and revoke refresh token.
    /// </summary>
    [Authorize]
    [SwaggerOperation(
        Summary = "Logout",
        Description = "Revokes the provided refresh token."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Logged out")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation error", typeof(Error))]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Error?.Type switch
        {
            ErrorType.Validation => BadRequest(result.Error),
            ErrorType.Unauthorized => Unauthorized(result.Error),
            ErrorType.Conflict => Conflict(result.Error),
            ErrorType.NotFound => NotFound(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Error)
        };
    }

    private IActionResult ToActionResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok();
        }

        return result.Error?.Type switch
        {
            ErrorType.Validation => BadRequest(result.Error),
            ErrorType.Unauthorized => Unauthorized(result.Error),
            ErrorType.Conflict => Conflict(result.Error),
            ErrorType.NotFound => NotFound(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Error)
        };
    }
}

