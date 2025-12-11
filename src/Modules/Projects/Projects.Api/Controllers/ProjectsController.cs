using Common.Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using Projects.Application.Commands.ChangeProjectStatus;
using Projects.Application.Commands.CreateProject;
using Projects.Application.Commands.DeleteProject;
using Projects.Application.Commands.UpdateProject;
using Projects.Application.DTOs;
using Projects.Application.Queries.GetProjectById;
using Projects.Application.Queries.GetProjects;

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
    [ProducesResponseType(typeof(IEnumerable<ProjectListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProjectListItemDto>>> GetProjects(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetProjectsQuery(), cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return Ok(result.Value?.ToList() ?? new List<ProjectListItemDto>());
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
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProjectDto>> GetById(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetProjectByIdQuery(projectId), cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Error?.Type == Common.Domain.Results.ErrorType.NotFound
                ? NotFound(result.Error.Description)
                : Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return Ok(result.Value!);
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
    [ProducesResponseType(typeof(CreateProjectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateProjectResponse>> Create([FromBody] CreateProjectCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send<CreateProjectCommand, CreateProjectResponse>(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return CreatedAtAction(nameof(GetById), new { projectId = result.Value!.ProjectId }, result.Value);
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
    public async Task<IActionResult> Update(Guid projectId, [FromBody] UpdateProjectCommand body, CancellationToken cancellationToken)
    {
        var command = body with { ProjectId = projectId };
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
    public async Task<IActionResult> ChangeStatus(Guid projectId, [FromBody] ChangeProjectStatusCommand body, CancellationToken cancellationToken)
    {
        var command = body with { ProjectId = projectId };
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
    public async Task<IActionResult> Delete(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteProjectCommand(projectId), cancellationToken);
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

