using Common.Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using AccessPermissions.Application.Commands.CreateAccessPermission;
using AccessPermissions.Application.Commands.DeleteAccessPermission;
using AccessPermissions.Application.Commands.UpdateAccessPermission;
using AccessPermissions.Application.DTOs;
using AccessPermissions.Application.Queries.GetAccessPermissionById;
using AccessPermissions.Application.Queries.GetAccessPermissions;
using AccessPermissions.Application.Queries.GetUserPermissions;
using Contracts.AccessPermissions;

namespace AccessPermissions.Api.Controllers;

/// <summary>
/// Контроллер для управления разрешениями доступа
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AccessPermissionsController : ControllerBase
{
    private readonly ISender _sender;

    public AccessPermissionsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Создать новое разрешение доступа
    /// </summary>
    /// <param name="command">Данные для создания разрешения</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Идентификатор созданного разрешения</returns>
    /// <response code="201">Разрешение успешно создано</response>
    /// <response code="400">Ошибка валидации или дубликат разрешения</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateAccessPermissionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateAccessPermissionResponse>> Create([FromBody] CreateAccessPermissionCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send<CreateAccessPermissionCommand, CreateAccessPermissionResponse>(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return CreatedAtAction(nameof(GetById), new { permissionId = result.Value!.PermissionId }, result.Value);
    }

    /// <summary>
    /// Получить разрешение доступа по идентификатору
    /// </summary>
    /// <param name="permissionId">Идентификатор разрешения</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о разрешении</returns>
    /// <response code="200">Разрешение найдено</response>
    /// <response code="404">Разрешение не найдено</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("{permissionId:guid}")]
    [ProducesResponseType(typeof(AccessPermissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AccessPermissionDto>> GetById(Guid permissionId, CancellationToken cancellationToken)
    {
        var query = new GetAccessPermissionByIdQuery(permissionId);
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
    /// Получить список разрешений доступа с фильтрацией
    /// </summary>
    /// <param name="tenantId">Идентификатор тенанта (опционально)</param>
    /// <param name="userId">Идентификатор пользователя (опционально)</param>
    /// <param name="projectId">Идентификатор проекта (опционально)</param>
    /// <param name="taskId">Идентификатор задачи (опционально)</param>
    /// <param name="scope">Область действия разрешения (опционально)</param>
    /// <param name="action">Действие разрешения (опционально)</param>
    /// <param name="type">Тип разрешения (опционально)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список разрешений</returns>
    /// <response code="200">Список разрешений получен</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccessPermissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AccessPermissionDto>>> GetPermissions(
        [FromQuery] Guid? tenantId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] Guid? projectId = null,
        [FromQuery] Guid? taskId = null,
        [FromQuery] PermissionScopeContract? scope = null,
        [FromQuery] PermissionActionContract? action = null,
        [FromQuery] PermissionTypeContract? type = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAccessPermissionsQuery(
            tenantId,
            userId,
            projectId,
            taskId,
            scope.HasValue ? (AccessPermissions.Domain.Enums.PermissionScope)(int)scope.Value : null,
            action.HasValue ? (AccessPermissions.Domain.Enums.PermissionAction)(int)action.Value : null,
            type.HasValue ? (AccessPermissions.Domain.Enums.PermissionType)(int)type.Value : null);

        var result = await _sender.Send(query, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return Ok(result.Value?.ToList() ?? new List<AccessPermissionDto>());
    }

    /// <summary>
    /// Получить все разрешения пользователя в определенном тенанте или проекте
    /// </summary>
    /// <param name="tenantId">Идентификатор тенанта</param>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="projectId">Идентификатор проекта (опционально)</param>
    /// <param name="taskId">Идентификатор задачи (опционально)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список разрешений пользователя</returns>
    /// <response code="200">Список разрешений получен</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("users/{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<AccessPermissionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AccessPermissionDto>>> GetUserPermissions(
        [FromRoute] Guid userId,
        [FromQuery] Guid tenantId,
        [FromQuery] Guid? projectId = null,
        [FromQuery] Guid? taskId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserPermissionsQuery(userId, tenantId, projectId);
        var result = await _sender.Send(query, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return Ok(result.Value?.ToList() ?? new List<AccessPermissionDto>());
    }

    /// <summary>
    /// Обновить разрешение доступа
    /// </summary>
    /// <param name="permissionId">Идентификатор разрешения</param>
    /// <param name="body">Данные для обновления</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Разрешение успешно обновлено</response>
    /// <response code="404">Разрешение не найдено</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPut("{permissionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid permissionId, [FromBody] UpdateAccessPermissionRequest body, CancellationToken cancellationToken)
    {
        var command = new UpdateAccessPermissionCommand(
            permissionId,
            body.Type.HasValue ? (AccessPermissions.Domain.Enums.PermissionType?)(int)body.Type.Value : null,
            body.ExpiresAt,
            body.Note);

        var result = await _sender.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return NoContent();
    }

    /// <summary>
    /// Удалить разрешение доступа
    /// </summary>
    /// <param name="permissionId">Идентификатор разрешения</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Разрешение успешно удалено</response>
    /// <response code="404">Разрешение не найдено</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpDelete("{permissionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid permissionId, CancellationToken cancellationToken)
    {
        var command = new DeleteAccessPermissionCommand(permissionId);
        var result = await _sender.Send(command, cancellationToken);
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

/// <summary>
/// Запрос на обновление разрешения доступа
/// </summary>
public record UpdateAccessPermissionRequest(
    PermissionTypeContract? Type = null,
    DateTime? ExpiresAt = null,
    string? Note = null);

