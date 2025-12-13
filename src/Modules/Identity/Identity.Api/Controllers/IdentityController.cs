using Common.Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using Identity.Application.Commands.ChangePassword;
using Identity.Application.Commands.Login;
using Identity.Application.Commands.Logout;
using Identity.Application.Commands.RefreshToken;
using Identity.Application.Queries.GetSession;
using Identity.Application.Queries.GetSessions;

namespace Identity.Api.Controllers;

/// <summary>
/// Контроллер для управления аутентификацией и авторизацией
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    private readonly ISender _sender;

    public IdentityController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Вход пользователя в систему
    /// </summary>
    /// <param name="command">Данные для входа (TenantId, Email, Password)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Access token, Refresh token и информация о сессии</returns>
    /// <response code="200">Успешный вход</response>
    /// <response code="400">Неверные учетные данные или ошибка валидации</response>
    /// <response code="401">Неверный email или пароль</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var command = new LoginCommand(
            request.TenantId,
            request.Email,
            request.Password,
            ipAddress,
            userAgent);

        var result = await _sender.Send<LoginCommand, LoginResponse>(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return Ok(result.Value!);
    }

    /// <summary>
    /// Обновление access token с помощью refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Новый access token и refresh token</returns>
    /// <response code="200">Токены успешно обновлены</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="401">Неверный или истекший refresh token</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var command = new RefreshTokenCommand(request.RefreshToken, ipAddress, userAgent);
        var result = await _sender.Send<RefreshTokenCommand, RefreshTokenResponse>(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return Ok(result.Value!);
    }

    /// <summary>
    /// Изменение пароля пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="request">Текущий и новый пароль</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Пароль успешно изменен</response>
    /// <response code="400">Ошибка валидации или неверный текущий пароль</response>
    /// <response code="404">Пользователь не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost("{userId:guid}/change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePassword(Guid userId, [FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var command = new ChangePasswordCommand(
            userId,
            request.CurrentPassword,
            request.NewPassword,
            request.RevokeAllSessions,
            ipAddress,
            userAgent);

        var result = await _sender.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return NoContent();
    }

    /// <summary>
    /// Выход пользователя из системы
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии</param>
    /// <param name="request">Причина выхода (опционально)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Выход выполнен успешно</response>
    /// <response code="404">Сессия не найдена</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost("logout/{sessionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout(Guid sessionId, [FromBody] LogoutRequest? request = null, CancellationToken cancellationToken = default)
    {
        var command = new LogoutCommand(sessionId, request?.Reason);
        var result = await _sender.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return NoContent();
    }

    /// <summary>
    /// Получить информацию о сессии
    /// </summary>
    /// <param name="sessionId">Идентификатор сессии</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о сессии</returns>
    /// <response code="200">Сессия найдена</response>
    /// <response code="404">Сессия не найдена</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("sessions/{sessionId:guid}")]
    [ProducesResponseType(typeof(Identity.Application.DTOs.SessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Identity.Application.DTOs.SessionDto>> GetSession(Guid sessionId, CancellationToken cancellationToken)
    {
        var query = new GetSessionQuery(sessionId);
        var result = await _sender.Send(query, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Error?.Type == Common.Domain.Results.ErrorType.NotFound
                ? NotFound(result.Error.Description)
                : Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return Ok(result.Value!);
    }

    /// <summary>
    /// Получить список сессий пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список сессий</returns>
    /// <response code="200">Список сессий получен</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("users/{userId:guid}/sessions")]
    [ProducesResponseType(typeof(IEnumerable<Identity.Application.DTOs.SessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Identity.Application.DTOs.SessionDto>>> GetSessions(Guid userId, CancellationToken cancellationToken)
    {
        var query = new GetSessionsQuery(userId);
        var result = await _sender.Send(query, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return Ok(result.Value?.ToList() ?? new List<Identity.Application.DTOs.SessionDto>());
    }

    private static int? MapStatus(Common.Domain.Results.Error? error)
    {
        if (error == null) return 500;
        return error.Type switch
        {
            Common.Domain.Results.ErrorType.Validation => 400,
            Common.Domain.Results.ErrorType.Unauthorized => 401,
            Common.Domain.Results.ErrorType.Forbidden => 403,
            Common.Domain.Results.ErrorType.NotFound => 404,
            Common.Domain.Results.ErrorType.Conflict => 409,
            _ => 500
        };
    }
}

/// <summary>
/// Запрос на вход в систему
/// </summary>
public record LoginRequest(
    Guid TenantId,
    string Email,
    string Password);

/// <summary>
/// Запрос на обновление токена
/// </summary>
public record RefreshTokenRequest(
    string RefreshToken);

/// <summary>
/// Запрос на изменение пароля
/// </summary>
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    bool RevokeAllSessions = true);

/// <summary>
/// Запрос на выход из системы
/// </summary>
public record LogoutRequest(
    string? Reason = null);

