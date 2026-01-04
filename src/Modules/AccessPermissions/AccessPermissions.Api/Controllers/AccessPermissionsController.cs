using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.AspNetCore.Mvc;
using AccessPermissions.Application.Commands.CreateAccessPermission;
using AccessPermissions.Application.Commands.DeleteAccessPermission;
using AccessPermissions.Application.Commands.UpdateAccessPermission;
using AccessPermissions.Application.Queries.GetAccessPermissionById;
using AccessPermissions.Application.Queries.GetAccessPermissions;
using AccessPermissions.Application.Queries.GetUserPermissions;
using AccessPermissions.Contracts.Api.Requests;
using AccessPermissions.Contracts.Api.Responses;
using AccessPermissions.Contracts.Enums;
using AccessPermissions.Domain.Enums;

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
    public async Task<ActionResult<CreateAccessPermissionResponse>> Create(
        [FromBody] CreateAccessPermissionRequest request,
        [FromHeader(Name = "X-Tenant-Id")] int tenantId,
        CancellationToken cancellationToken)
    {
        var command = new CreateAccessPermissionCommand(
            tenantId,
            request.UserId,
            (PermissionScope)(int)request.Scope,
            (PermissionAction)(int)request.Action,
            (PermissionType)(int)request.Type,
            request.CreatedByUserId,
            request.ProjectId,
            request.TaskId,
            request.ExpiresAt,
            request.Note);

        var result = await _sender.Send<CreateAccessPermissionCommand, CreateAccessPermissionResponse>(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return CreatedAtAction(nameof(GetById), new { permissionId = result.Value!.PermissionId, tenantId }, result.Value);
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
    [ProducesResponseType(typeof(AccessPermissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AccessPermissionResponse>> GetById(
        Guid permissionId,
        [FromHeader(Name = "X-Tenant-Id")] int tenantId,
        CancellationToken cancellationToken)
    {
        var query = new GetAccessPermissionByIdQuery(permissionId, tenantId);
        var result = await _sender.Send(query, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Error?.Type == ErrorType.NotFound
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
    [ProducesResponseType(typeof(IEnumerable<AccessPermissionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AccessPermissionResponse>>> GetPermissions(
        [FromHeader(Name = "X-Tenant-Id")] int tenantId,
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
            scope.HasValue ? (PermissionScope)(int)scope.Value : null,
            action.HasValue ? (PermissionAction)(int)action.Value : null,
            type.HasValue ? (PermissionType)(int)type.Value : null);

        var result = await _sender.Send(query, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return Ok(result.Value?.ToList() ?? new List<AccessPermissionResponse>());
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
    [ProducesResponseType(typeof(IEnumerable<AccessPermissionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AccessPermissionResponse>>> GetUserPermissions(
        [FromRoute] Guid userId,
        [FromHeader(Name = "X-Tenant-Id")] int tenantId,
        [FromQuery] Guid? projectId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserPermissionsQuery(userId, tenantId, projectId);
        var result = await _sender.Send(query, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return Ok(result.Value?.ToList() ?? new List<AccessPermissionResponse>());
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
    public async Task<IActionResult> Update(
        Guid permissionId,
        [FromHeader(Name = "X-Tenant-Id")] int tenantId,
        [FromBody] UpdateAccessPermissionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAccessPermissionCommand(
            permissionId,
            tenantId,
            request.Type.HasValue ? (PermissionType?)(int)request.Type.Value : null,
            request.ExpiresAt,
            request.Note);

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
    public async Task<IActionResult> Delete(
        Guid permissionId,
        [FromHeader(Name = "X-Tenant-Id")] int tenantId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteAccessPermissionCommand(permissionId, tenantId);
        var result = await _sender.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return NoContent();
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

