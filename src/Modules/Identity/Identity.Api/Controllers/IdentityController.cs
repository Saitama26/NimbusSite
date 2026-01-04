using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.AspNetCore.Mvc;
using Identity.Application.Commands.ChangePassword;
using Identity.Application.Commands.Login;
using Identity.Application.Commands.Logout;
using Identity.Application.Commands.RefreshToken;
using Identity.Application.Queries.GetSession;
using Identity.Application.Queries.GetSessions;
using Identity.Contracts.Api.Requests;
using Identity.Contracts.Api.Responses;
using Identity.Contracts.Enums;

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
    /// <param name="tenantId">Идентификатор тенанта (query параметр)</param>
    /// <param name="request">Данные для входа (Email, Password)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Access token, Refresh token и информация о сессии</returns>
    /// <response code="200">Успешный вход</response>
    /// <response code="400">Неверные учетные данные или ошибка валидации</response>
    /// <response code="401">Неверный email или пароль</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(Identity.Contracts.Api.Responses.LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Identity.Contracts.Api.Responses.LoginResponse>> Login(
        [FromQuery] int tenantId,
        [FromBody] LoginRequest request, 
        CancellationToken cancellationToken)
    {
        if (tenantId <= 0)
        {
            return BadRequest(new { error = "TenantId is required. Provide tenantId as query parameter: ?tenantId=1" });
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var command = new LoginCommand(
            tenantId,
            request.Email,
            request.Password,
            ipAddress,
            userAgent);

        var result = await _sender.Send<LoginCommand, Identity.Application.Commands.Login.LoginResponse>(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        var loginResponse = result.Value!;
        var response = new Identity.Contracts.Api.Responses.LoginResponse(
            loginResponse.AccessToken,
            loginResponse.RefreshToken,
            "Bearer",
            loginResponse.ExpiresIn,
            loginResponse.RefreshTokenExpiresAt,
            loginResponse.SessionId);

        return Ok(response);
    }

    /// <summary>
    /// Обновление access token с помощью refresh token
    /// </summary>
    /// <param name="tenantId">Числовой идентификатор тенанта (query параметр)</param>
    /// <param name="request">Refresh token в теле запроса</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Новый access token и refresh token</returns>
    /// <response code="200">Токены успешно обновлены</response>
    /// <response code="400">Ошибка валидации (не указан tenantId или неверный формат)</response>
    /// <response code="401">Неверный или истекший refresh token</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(Identity.Contracts.Api.Responses.RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Identity.Contracts.Api.Responses.RefreshTokenResponse>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        [FromQuery] int tenantId,
        CancellationToken cancellationToken)
    {
        if (tenantId <= 0)
        {
            return BadRequest(new { error = "TenantId is required. Provide tenantId as query parameter: ?tenantId=1" });
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var command = new RefreshTokenCommand(request.RefreshToken, tenantId, ipAddress, userAgent);
        var result = await _sender.Send<RefreshTokenCommand, Identity.Application.Commands.RefreshToken.RefreshTokenResponse>(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        var refreshResponse = result.Value!;
        var response = new Identity.Contracts.Api.Responses.RefreshTokenResponse(
            refreshResponse.AccessToken,
            refreshResponse.RefreshToken,
            "Bearer",
            refreshResponse.ExpiresIn,
            refreshResponse.RefreshTokenExpiresAt);

        return Ok(response);
    }

    /// <summary>
    /// Изменение пароля пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="tenantId">Числовой идентификатор тенанта (query параметр)</param>
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
    public async Task<IActionResult> ChangePassword(Guid userId, [FromBody] ChangePasswordRequest request, [FromQuery] int tenantId, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

        var command = new ChangePasswordCommand(
            userId,
            tenantId,
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
    /// <param name="tenantId">Числовой идентификатор тенанта (query параметр)</param>
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
    public async Task<IActionResult> Logout(
        Guid sessionId,
        [FromQuery] int tenantId,
        [FromBody] Identity.Contracts.Api.Requests.LogoutRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        if (tenantId <= 0)
        {
            return BadRequest(new { error = "TenantId is required. Provide tenantId as query parameter: ?tenantId=1" });
        }

        var command = new LogoutCommand(sessionId, tenantId, request?.Reason);
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
    /// <param name="tenantId">Числовой идентификатор тенанта (query параметр)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о сессии</returns>
    /// <response code="200">Сессия найдена</response>
    /// <response code="404">Сессия не найдена</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("sessions/{sessionId:guid}")]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SessionResponse>> GetSession(
        Guid sessionId,
        [FromQuery] int tenantId,
        CancellationToken cancellationToken)
    {
        if (tenantId <= 0)
        {
            return BadRequest(new { error = "TenantId is required. Provide tenantId as query parameter: ?tenantId=1" });
        }

        var query = new GetSessionQuery(sessionId, tenantId);
        var result = await _sender.Send(query, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Error?.Type == ErrorType.NotFound
                ? NotFound(result.Error.Description)
                : Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        var dto = result.Value!;
        var response = new SessionResponse(
            dto.Id,
            dto.UserId,
            dto.TenantId,
            (SessionStatusContract)(int)dto.Status,
            dto.IpAddress,
            dto.UserAgent,
            dto.LastActivityAt,
            dto.CreatedAt,
            dto.ExpiresAt);

        return Ok(response);
    }

    /// <summary>
    /// Получить список сессий пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="tenantId">Числовой идентификатор тенанта (query параметр)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список сессий</returns>
    /// <response code="200">Список сессий получен</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("users/{userId:guid}/sessions")]
    [ProducesResponseType(typeof(IEnumerable<SessionListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SessionListResponse>>> GetSessions(Guid userId, [FromQuery] int tenantId, CancellationToken cancellationToken)
    {
        var query = new GetSessionsQuery(userId, tenantId);
        var result = await _sender.Send(query, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        var responses = result.Value!.Select(dto => new SessionListResponse(
            dto.Id,
            dto.UserId,
            dto.TenantId,
            (SessionStatusContract)(int)dto.Status,
            dto.IpAddress,
            dto.UserAgent,
            dto.LastActivityAt,
            dto.CreatedAt));

        return Ok(responses.ToList());
    }

    private static int? MapStatus(Error? error)
    {
        if (error == null) return 500;
        return error.Type switch
        {
            ErrorType.Validation => 400,
            ErrorType.Unauthorized => 401,
            ErrorType.Forbidden => 403,
            ErrorType.NotFound => 404,
            ErrorType.Conflict => 409,
            _ => 500
        };
    }
}

