using Common.Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using Tasks.Application.Commands.AssignTask;
using Tasks.Application.Commands.ChangeTaskStatus;
using Tasks.Application.Commands.CreateTask;
using Tasks.Application.Commands.DeleteTask;
using Tasks.Application.Commands.UpdateTask;
using Tasks.Application.DTOs;
using Tasks.Application.Queries.GetTaskById;
using Tasks.Application.Queries.GetTasks;

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
    [ProducesResponseType(typeof(IEnumerable<TaskListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TaskListItemDto>>> GetTasks(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTasksQuery(), cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return Ok(result.Value?.ToList() ?? new List<TaskListItemDto>());
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
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TaskDto>> GetById(Guid taskId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTaskByIdQuery(taskId), cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Error?.Type == Common.Domain.Results.ErrorType.NotFound
                ? NotFound(result.Error.Description)
                : Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return Ok(result.Value!);
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
    [ProducesResponseType(typeof(CreateTaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateTaskResponse>> Create([FromBody] CreateTaskCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send<CreateTaskCommand, CreateTaskResponse>(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        return CreatedAtAction(nameof(GetById), new { taskId = result.Value!.TaskId }, result.Value);
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
    public async Task<IActionResult> Update(Guid taskId, [FromBody] UpdateTaskCommand body, CancellationToken cancellationToken)
    {
        var command = body with { TaskId = taskId };
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
    public async Task<IActionResult> ChangeStatus(Guid taskId, [FromBody] ChangeTaskStatusCommand body, CancellationToken cancellationToken)
    {
        var command = body with { TaskId = taskId };
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
    public async Task<IActionResult> Assign(Guid taskId, [FromBody] AssignTaskCommand body, CancellationToken cancellationToken)
    {
        var command = body with { TaskId = taskId };
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
    public async Task<IActionResult> Delete(Guid taskId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteTaskCommand(taskId), cancellationToken);
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

