using Common.Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using Users.Application.Commands.ChangeUserStatus;
using Users.Application.Commands.CreateUser;
using Users.Application.Commands.DeleteUser;
using Users.Application.Commands.RegisterUser;
using Users.Application.Commands.UpdateUser;
using Users.Application.Queries.GetUserById;
using Users.Application.Queries.GetUsers;
using Users.Contracts.Api.Requests;
using Users.Contracts.Api.Responses;
using Users.Contracts.Enums;

namespace Users.Api.Controllers;

/// <summary>
/// Контроллер для управления пользователями
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Получить список всех пользователей
    /// </summary>
    /// <param name="tenantId">Числовой идентификатор тенанта (query параметр, обязательный)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список пользователей</returns>
    /// <response code="200">Успешно получен список пользователей</response>
    /// <response code="400">Не указан или неверный tenantId</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserListResponse>>> GetUsers([FromQuery] int tenantId, CancellationToken cancellationToken)
    {
        if (tenantId <= 0)
        {
            return Problem("TenantId is required. Provide tenantId as query parameter: ?tenantId=1", statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await _sender.Send(new GetUsersQuery(tenantId), cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        var responses = result.Value?.Select(u => new UserListResponse(
            u.Id,
            u.Email,
            u.Name,
            (UserStatusContract)(int)u.Status,
            u.CreatedAt)) ?? new List<UserListResponse>();

        return Ok(responses);
    }

    /// <summary>
    /// Получить пользователя по идентификатору
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="tenantId">Числовой идентификатор тенанта (query параметр)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о пользователе</returns>
    /// <response code="200">Пользователь успешно найден</response>
    /// <response code="404">Пользователь не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponse>> GetById(Guid userId, [FromQuery] int tenantId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserByIdQuery(userId, tenantId), cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Error?.Type == Common.Domain.Results.ErrorType.NotFound
                ? NotFound(result.Error.Description)
                : Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        var dto = result.Value!;
        var response = new UserResponse(
            dto.Id,
            dto.Email,
            dto.Name,
            (UserStatusContract)(int)dto.Status,
            dto.CreatedAt,
            dto.UpdatedAt,
            dto.LastLoginAt,
            dto.Phone,
            dto.Bio);

        return Ok(response);
    }

    /// <summary>
    /// Зарегистрировать нового пользователя (с паролем)
    /// </summary>
    /// <param name="tenantId">Числовой идентификатор тенанта (query параметр)</param>
    /// <param name="request">Данные для регистрации пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Идентификатор созданного пользователя</returns>
    /// <response code="201">Пользователь успешно зарегистрирован</response>
    /// <response code="400">Ошибка валидации (пустой email, неверный формат email, email уже существует, слабый пароль)</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(Users.Contracts.Api.Responses.RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Users.Contracts.Api.Responses.RegisterUserResponse>> Register([FromBody] RegisterUserRequest request, [FromQuery] int tenantId, CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            tenantId,
            request.Email,
            request.Password,
            request.Name,
            request.Phone,
            request.Bio);

        var result = await _sender.Send<RegisterUserCommand, Users.Application.Commands.RegisterUser.RegisterUserResponse>(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        var response = new Users.Contracts.Api.Responses.RegisterUserResponse(result.Value!.UserId);
        return CreatedAtAction(nameof(GetById), new { userId = response.UserId }, response);
    }

    /// <summary>
    /// Создать нового пользователя (без пароля, для администраторов)
    /// </summary>
    /// <param name="tenantId">Числовой идентификатор тенанта (query параметр)</param>
    /// <param name="request">Данные для создания пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Идентификатор созданного пользователя</returns>
    /// <response code="201">Пользователь успешно создан</response>
    /// <response code="400">Ошибка валидации (пустой email, неверный формат email, email уже существует)</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost]
    [ProducesResponseType(typeof(Users.Contracts.Api.Responses.CreateUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Users.Contracts.Api.Responses.CreateUserResponse>> Create([FromBody] CreateUserRequest request, [FromQuery] int tenantId, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(
            tenantId,
            request.Email,
            request.Name,
            request.Phone,
            request.Bio);

        var result = await _sender.Send<CreateUserCommand, Users.Application.Commands.CreateUser.CreateUserResponse>(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        var response = new Users.Contracts.Api.Responses.CreateUserResponse(result.Value!.UserId);
        return CreatedAtAction(nameof(GetById), new { userId = response.UserId }, response);
    }

    /// <summary>
    /// Обновить информацию о пользователе
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="tenantId">Числовой идентификатор тенанта (query параметр)</param>
    /// <param name="request">Данные для обновления (имя, телефон, биография)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Пользователь успешно обновлен</response>
    /// <response code="404">Пользователь не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPut("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid userId, [FromBody] UpdateUserRequest request, [FromQuery] int tenantId, CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(
            userId,
            tenantId,
            request.Name,
            request.Phone,
            request.Bio);

        var result = await _sender.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return NoContent();
    }

    /// <summary>
    /// Изменить статус пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="tenantId">Числовой идентификатор тенанта (query параметр)</param>
    /// <param name="request">Новый статус пользователя (Active, Inactive, Deleted)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Статус пользователя успешно изменен</response>
    /// <response code="404">Пользователь не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPatch("{userId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeStatus(Guid userId, [FromBody] ChangeUserStatusRequest request, [FromQuery] int tenantId, CancellationToken cancellationToken)
    {
        var command = new ChangeUserStatusCommand(
            userId,
            tenantId,
            (Users.Domain.Enums.UserStatus)(int)request.Status);

        var result = await _sender.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return NoContent();
    }

    /// <summary>
    /// Удалить пользователя (soft delete - меняет статус на Deleted)
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="tenantId">Числовой идентификатор тенанта (query параметр)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Пользователь успешно удален</response>
    /// <response code="400">Пользователь уже удален</response>
    /// <response code="404">Пользователь не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpDelete("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid userId, [FromQuery] int tenantId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteUserCommand(userId, tenantId), cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return NoContent();
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

