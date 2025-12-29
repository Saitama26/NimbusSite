using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.AspNetCore.Mvc;
using Projects.Application.Commands.AddUserToProject;
using Projects.Application.Commands.ChangeProjectStatus;
using Projects.Application.Commands.ChangeUserRoleInProject;
using Projects.Application.Commands.CreateProject;
using Projects.Application.Commands.DeleteProject;
using Projects.Application.Commands.RemoveUserFromProject;
using Projects.Application.Commands.UpdateProject;
using Projects.Application.Queries.GetProjectById;
using Projects.Application.Queries.GetProjects;
using Projects.Contracts.Api.Requests;
using Projects.Contracts.Api.Responses;
using Projects.Contracts.Enums;

namespace Projects.Api.Controllers;

/// <summary>
/// Контроллер для управления проектами
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ISender _sender;

    public ProjectsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Получить список всех проектов
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список проектов</returns>
    /// <response code="200">Успешно получен список проектов</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProjectListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProjectListResponse>>> GetProjects([FromQuery] int tenantId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetProjectsQuery(tenantId), cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        var responses = result.Value!.Select(dto => new ProjectListResponse(
            dto.Id,
            dto.TenantId,
            dto.Name,
            (ProjectStatusContract)(int)dto.Status,
            dto.UpdatedAt));

        return Ok(responses.ToList());
    }

    /// <summary>
    /// Получить проект по идентификатору
    /// </summary>
    /// <param name="projectId">Идентификатор проекта</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о проекте</returns>
    /// <response code="200">Проект успешно найден</response>
    /// <response code="404">Проект не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("{projectId:guid}")]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProjectResponse>> GetById(Guid projectId, [FromQuery] int tenantId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetProjectByIdQuery(projectId, tenantId), cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Error?.Type == ErrorType.NotFound
                ? NotFound(result.Error.Description)
                : Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        var dto = result.Value!;
        var response = new ProjectResponse(
            dto.Id,
            dto.TenantId,
            dto.Name,
            dto.Description,
            (ProjectStatusContract)(int)dto.Status,
            dto.CreatedAt,
            dto.UpdatedAt);

        return Ok(response);
    }

    /// <summary>
    /// Создать новый проект
    /// </summary>
    /// <param name="command">Данные для создания проекта</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Идентификатор созданного проекта</returns>
    /// <response code="201">Проект успешно создан</response>
    /// <response code="400">Ошибка валидации (пустое имя или имя уже существует)</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost]
    [ProducesResponseType(typeof(Projects.Contracts.Api.Responses.CreateProjectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Projects.Contracts.Api.Responses.CreateProjectResponse>> Create(
        [FromBody] CreateProjectRequest request,
        [FromQuery] int tenantId,
        [FromQuery] Guid createdByUserId,
        CancellationToken cancellationToken)
    {
        // TODO: Извлечь createdByUserId из JWT токена
        var command = new CreateProjectCommand(
            tenantId,
            createdByUserId,
            request.Name,
            request.Description);

        var result = await _sender.Send<CreateProjectCommand, Projects.Application.Commands.CreateProject.CreateProjectResponse>(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        var response = new Projects.Contracts.Api.Responses.CreateProjectResponse(result.Value!.ProjectId);
        return CreatedAtAction(nameof(GetById), new { projectId = response.ProjectId, tenantId = tenantId }, response);
    }

    /// <summary>
    /// Обновить информацию о проекте
    /// </summary>
    /// <param name="projectId">Идентификатор проекта</param>
    /// <param name="body">Данные для обновления (имя и/или описание)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Проект успешно обновлен</response>
    /// <response code="404">Проект не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPut("{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        Guid projectId,
        [FromBody] UpdateProjectRequest request,
        [FromQuery] int tenantId,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProjectCommand(
            projectId,
            tenantId,
            request.Name,
            request.Description);
        var result = await _sender.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return NoContent();
    }

    /// <summary>
    /// Изменить статус проекта
    /// </summary>
    /// <param name="projectId">Идентификатор проекта</param>
    /// <param name="body">Новый статус проекта (Active, Archived, Deleted)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Статус проекта успешно изменен</response>
    /// <response code="404">Проект не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPatch("{projectId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeStatus(
        Guid projectId,
        [FromBody] ChangeProjectStatusRequest request,
        [FromQuery] int tenantId,
        CancellationToken cancellationToken)
    {
        var command = new ChangeProjectStatusCommand(
            projectId,
            tenantId,
            (Projects.Domain.Enums.ProjectStatus)(int)request.Status);
        var result = await _sender.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return NoContent();
    }

    /// <summary>
    /// Удалить проект (soft delete - меняет статус на Deleted)
    /// </summary>
    /// <param name="projectId">Идентификатор проекта</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Проект успешно удален</response>
    /// <response code="400">Проект уже удален</response>
    /// <response code="404">Проект не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpDelete("{projectId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid projectId, [FromQuery] int tenantId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteProjectCommand(projectId, tenantId), cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return NoContent();
    }

    /// <summary>
    /// Добавить пользователя в проект
    /// </summary>
    /// <param name="projectId">Идентификатор проекта</param>
    /// <param name="body">Данные для добавления пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="201">Пользователь успешно добавлен в проект</response>
    /// <response code="400">Ошибка валидации или пользователь уже в проекте</response>
    /// <response code="404">Проект не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost("{projectId:guid}/users")]
    [ProducesResponseType(typeof(AddUserToProjectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AddUserToProjectResponse>> AddUser(
        Guid projectId,
        [FromBody] AddUserToProjectRequest request,
        [FromQuery] int tenantId,
        CancellationToken cancellationToken)
    {
        var command = new AddUserToProjectCommand(
            projectId,
            tenantId,
            request.UserId,
            (Users.Domain.Enums.UserRole)(int)request.Role);
        var result = await _sender.Send<AddUserToProjectCommand, AddUserToProjectResponse>(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return CreatedAtAction(nameof(GetById), new { projectId }, result.Value);
    }

    /// <summary>
    /// Удалить пользователя из проекта
    /// </summary>
    /// <param name="projectId">Идентификатор проекта</param>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Пользователь успешно удален из проекта</response>
    /// <response code="404">Проект или пользователь не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpDelete("{projectId:guid}/users/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveUser(
        Guid projectId, 
        Guid userId, 
        [FromQuery] int tenantId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveUserFromProjectCommand(projectId, tenantId, userId);
        var result = await _sender.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return NoContent();
    }

    /// <summary>
    /// Изменить роль пользователя в проекте
    /// </summary>
    /// <param name="projectId">Идентификатор проекта</param>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="body">Новая роль</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Роль пользователя успешно изменена</response>
    /// <response code="404">Проект или пользователь не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPatch("{projectId:guid}/users/{userId:guid}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeUserRole(
        Guid projectId,
        Guid userId,
        [FromBody] ChangeUserRoleInProjectRequest request,
        [FromQuery] int tenantId,
        CancellationToken cancellationToken)
    {
        var command = new ChangeUserRoleInProjectCommand(
            projectId,
            tenantId,
            userId,
            (Users.Domain.Enums.UserRole)(int)request.Role);
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

