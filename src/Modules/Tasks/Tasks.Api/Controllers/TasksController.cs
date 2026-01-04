using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.AspNetCore.Mvc;
using Tasks.Application.Commands.AssignTask;
using Tasks.Application.Commands.ChangeTaskStatus;
using Tasks.Application.Commands.CreateTask;
using Tasks.Application.Commands.DeleteTask;
using Tasks.Application.Commands.UpdateTask;
using Tasks.Application.Queries.GetTaskById;
using Tasks.Application.Queries.GetTasks;
using Tasks.Contracts.Api.Requests;
using Tasks.Contracts.Api.Responses;
using Tasks.Contracts.Enums;

namespace Tasks.Api.Controllers;

/// <summary>
/// Контроллер для управления задачами
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ISender _sender;

    public TasksController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Получить список всех задач
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Список задач</returns>
    /// <response code="200">Успешно получен список задач</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TaskListResponse>>> GetTasks([FromQuery] int tenantId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTasksQuery(tenantId), cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        var responses = result.Value!.Select(dto => new TaskListResponse(
            dto.Id,
            dto.ProjectId,
            dto.Title,
            (TaskStatusContract)(int)dto.Status,
            (TaskPriorityContract)(int)dto.Priority,
            dto.AssignedToUserId,
            dto.DueDate,
            dto.CreatedAt));

        return Ok(responses.ToList());
    }

    /// <summary>
    /// Получить задачу по идентификатору
    /// </summary>
    /// <param name="taskId">Идентификатор задачи</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о задаче</returns>
    /// <response code="200">Задача успешно найдена</response>
    /// <response code="404">Задача не найдена</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpGet("{taskId:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskResponse>> GetById(Guid taskId, [FromQuery] int tenantId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTaskByIdQuery(taskId, tenantId), cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Error?.Type == ErrorType.NotFound
                ? NotFound(result.Error.Description)
                : Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        var dto = result.Value!;
        var response = new TaskResponse(
            dto.Id,
            dto.TenantId,
            dto.ProjectId,
            dto.Title,
            dto.Description,
            (TaskStatusContract)(int)dto.Status,
            (TaskPriorityContract)(int)dto.Priority,
            dto.AssignedToUserId,
            dto.CreatedByUserId,
            dto.DueDate,
            dto.CreatedAt,
            dto.UpdatedAt,
            dto.StartedAt,
            dto.CompletedAt);

        return Ok(response);
    }

    /// <summary>
    /// Создать новую задачу
    /// </summary>
    /// <param name="command">Данные для создания задачи</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Идентификатор созданной задачи</returns>
    /// <response code="201">Задача успешно создана</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost]
    [ProducesResponseType(typeof(Tasks.Contracts.Api.Responses.CreateTaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Tasks.Contracts.Api.Responses.CreateTaskResponse>> Create(
        [FromBody] CreateTaskRequest request,
        [FromQuery] int tenantId,
        [FromQuery] Guid createdByUserId,
        CancellationToken cancellationToken)
    {
        // TODO: Извлечь createdByUserId из JWT токена
        var command = new CreateTaskCommand(
            tenantId,
            request.ProjectId,
            request.Title,
            createdByUserId,
            (Tasks.Domain.Enums.TaskPriority)(int)request.Priority,
            request.Description,
            request.AssignedToUserId,
            request.DueDate);

        var result = await _sender.Send<CreateTaskCommand, Tasks.Application.Commands.CreateTask.CreateTaskResponse>(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        var response = new Tasks.Contracts.Api.Responses.CreateTaskResponse(result.Value!.TaskId);
        return CreatedAtAction(nameof(GetById), new { taskId = response.TaskId, tenantId = tenantId }, response);
    }

    /// <summary>
    /// Обновить информацию о задаче
    /// </summary>
    /// <param name="taskId">Идентификатор задачи</param>
    /// <param name="body">Данные для обновления</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Задача успешно обновлена</response>
    /// <response code="404">Задача не найдена</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPut("{taskId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        Guid taskId,
        [FromBody] UpdateTaskRequest request,
        [FromQuery] int tenantId,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTaskCommand(
            taskId,
            tenantId,
            request.Title,
            request.Description,
            request.Priority.HasValue ? (Tasks.Domain.Enums.TaskPriority?)(int)request.Priority.Value : null,
            request.DueDate);
        var result = await _sender.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return NoContent();
    }

    /// <summary>
    /// Изменить статус задачи
    /// </summary>
    /// <param name="taskId">Идентификатор задачи</param>
    /// <param name="body">Новый статус задачи</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Статус задачи успешно изменен</response>
    /// <response code="404">Задача не найдена</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPatch("{taskId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeStatus(
        Guid taskId,
        [FromBody] ChangeTaskStatusRequest request,
        [FromQuery] int tenantId,
        CancellationToken cancellationToken)
    {
        var command = new ChangeTaskStatusCommand(
            taskId,
            tenantId,
            (Tasks.Domain.Enums.TaskStatus)(int)request.Status);
        var result = await _sender.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return NoContent();
    }

    /// <summary>
    /// Назначить задачу пользователю
    /// </summary>
    /// <param name="taskId">Идентификатор задачи</param>
    /// <param name="body">Данные для назначения</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Задача успешно назначена</response>
    /// <response code="404">Задача не найдена</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPatch("{taskId:guid}/assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Assign(
        Guid taskId,
        [FromBody] AssignTaskRequest request,
        [FromQuery] int tenantId,
        CancellationToken cancellationToken)
    {
        var command = new AssignTaskCommand(
            taskId,
            tenantId,
            request.AssignedToUserId);
        var result = await _sender.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return NoContent();
    }

    /// <summary>
    /// Удалить задачу (soft delete - меняет статус на Deleted)
    /// </summary>
    /// <param name="taskId">Идентификатор задачи</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Задача успешно удалена</response>
    /// <response code="400">Задача уже удалена</response>
    /// <response code="404">Задача не найдена</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpDelete("{taskId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid taskId, [FromQuery] int tenantId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteTaskCommand(taskId, tenantId), cancellationToken);
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

