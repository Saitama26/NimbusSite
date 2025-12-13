using Common.Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using Users.Application.Commands.ChangeUserStatus;
using Users.Application.Commands.CreateUser;
using Users.Application.Commands.DeleteUser;
using Users.Application.Commands.RegisterUser;
using Users.Application.Commands.UpdateUser;
using Users.Application.DTOs;
using Users.Application.Queries.GetUserById;
using Users.Application.Queries.GetUsers;

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
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список пользователей</returns>
    /// <response code="200">Успешно получен список пользователей</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserListItemDto>>> GetUsers(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUsersQuery(), cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return Ok(result.Value?.ToList() ?? new List<UserListItemDto>());
    }

    /// <summary>
    /// Получить пользователя по идентификатору
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о пользователе</returns>
    /// <response code="200">Пользователь успешно найден</response>
    /// <response code="404">Пользователь не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> GetById(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserByIdQuery(userId), cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Error?.Type == Common.Domain.Results.ErrorType.NotFound
                ? NotFound(result.Error.Description)
                : Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return Ok(result.Value!);
    }

    /// <summary>
    /// Зарегистрировать нового пользователя (с паролем)
    /// </summary>
    /// <param name="command">Данные для регистрации пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Идентификатор созданного пользователя</returns>
    /// <response code="201">Пользователь успешно зарегистрирован</response>
    /// <response code="400">Ошибка валидации (пустой email, неверный формат email, email уже существует, слабый пароль)</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RegisterUserResponse>> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send<RegisterUserCommand, RegisterUserResponse>(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return CreatedAtAction(nameof(GetById), new { userId = result.Value!.UserId }, result.Value);
    }

    /// <summary>
    /// Создать нового пользователя (без пароля, для администраторов)
    /// </summary>
    /// <param name="command">Данные для создания пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Идентификатор созданного пользователя</returns>
    /// <response code="201">Пользователь успешно создан</response>
    /// <response code="400">Ошибка валидации (пустой email, неверный формат email, email уже существует)</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateUserResponse>> Create([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send<CreateUserCommand, CreateUserResponse>(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return CreatedAtAction(nameof(GetById), new { userId = result.Value!.UserId }, result.Value);
    }

    /// <summary>
    /// Обновить информацию о пользователе
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="body">Данные для обновления (имя, телефон, биография)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Пользователь успешно обновлен</response>
    /// <response code="404">Пользователь не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPut("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid userId, [FromBody] UpdateUserCommand body, CancellationToken cancellationToken)
    {
        var command = body with { UserId = userId };
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
    /// <param name="body">Новый статус пользователя (Active, Inactive, Deleted)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Статус пользователя успешно изменен</response>
    /// <response code="404">Пользователь не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPatch("{userId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeStatus(Guid userId, [FromBody] ChangeUserStatusCommand body, CancellationToken cancellationToken)
    {
        var command = body with { UserId = userId };
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
    public async Task<IActionResult> Delete(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteUserCommand(userId), cancellationToken);
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

