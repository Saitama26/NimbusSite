using Application.Tasks.Commands.AssignTask;
using Application.Tasks.Commands.ChangeTaskStatus;
using Application.Tasks.Commands.CreateTask;
using Application.Tasks.Commands.DeleteTask;
using Application.Tasks.Commands.UpdateTask;
using Application.Tasks.Queries.GetTaskById;
using Application.Tasks.Queries.GetTasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Api.Controllers;

// Using aliases to resolve ambiguity between TaskResponse types from different queries
using GetTasksResponse = Application.Tasks.Queries.GetTasks.TaskResponse;
using GetTaskByIdResponse = Application.Tasks.Queries.GetTaskById.TaskResponse;

[ApiController]
[Route("api/tasks")]
[Authorize]
public sealed class TasksController : ControllerBase
{
    private readonly ISender _sender;

    public TasksController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all tasks, optionally filtered by project, assigned user, or tenant.
    /// </summary>
    /// <param name="projectId">Optional project ID to filter tasks.</param>
    /// <param name="assignedUserId">Optional assigned user ID to filter tasks.</param>
    /// <param name="tenantId">Optional tenant ID to filter tasks.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of tasks.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all tasks",
        Description = "Retrieves a list of all tasks. Can be filtered by ProjectId, AssignedUserId, or TenantId."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Tasks retrieved successfully", typeof(IReadOnlyList<GetTasksResponse>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized", typeof(Error))]
    public async Task<IActionResult> GetTasks(
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? assignedUserId,
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var query = new GetTasksQuery(projectId, assignedUserId, tenantId);
        var result = await _sender.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get a task by ID.
    /// </summary>
    /// <param name="id">The ID of the task to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The task details.</returns>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Get task by ID",
        Description = "Retrieves a single task by its unique identifier."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Task retrieved successfully", typeof(GetTaskByIdResponse))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Task not found", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized", typeof(Error))]
    public async Task<IActionResult> GetTaskById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTaskByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Create a new task.
    /// </summary>
    /// <param name="command">Task creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the newly created task.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new task",
        Description = "Creates a new task and assigns it to a user within a project."
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "Task created successfully", typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Project, tenant, or user not found", typeof(Error))]
    public async Task<IActionResult> CreateTask(
        [FromBody] CreateTaskCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        
        if (result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status201Created, result.Value);
        }
        
        return ToActionResult(result);
    }

    /// <summary>
    /// Update an existing task.
    /// </summary>
    /// <param name="id">The ID of the task to update.</param>
    /// <param name="command">Task update details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the updated task.</returns>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        Summary = "Update task",
        Description = "Updates an existing task. Only provided fields will be updated."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Task updated successfully", typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Task not found", typeof(Error))]
    public async Task<IActionResult> UpdateTask(
        Guid id,
        [FromBody] UpdateTaskCommand command,
        CancellationToken cancellationToken)
    {
        var updateCommand = command with { TaskId = id };
        var result = await _sender.Send(updateCommand, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Delete a task.
    /// </summary>
    /// <param name="id">The ID of the task to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the deleted task.</returns>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Delete task",
        Description = "Deletes a task permanently."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Task deleted successfully", typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Task not found", typeof(Error))]
    public async Task<IActionResult> DeleteTask(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTaskCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Assign a task to a user.
    /// </summary>
    /// <param name="id">The ID of the task.</param>
    /// <param name="command">Task assignment details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the updated task.</returns>
    [HttpPost("{id:guid}/assign")]
    [SwaggerOperation(
        Summary = "Assign task to user",
        Description = "Assigns a task to a specific user."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Task assigned successfully", typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Task or user not found", typeof(Error))]
    public async Task<IActionResult> AssignTask(
        Guid id,
        [FromBody] AssignTaskCommand command,
        CancellationToken cancellationToken)
    {
        var updateCommand = command with { TaskId = id };
        var result = await _sender.Send(updateCommand, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Change the status of a task.
    /// </summary>
    /// <param name="id">The ID of the task.</param>
    /// <param name="command">Status change details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the updated task.</returns>
    [HttpPatch("{id:guid}/status")]
    [SwaggerOperation(
        Summary = "Change task status",
        Description = "Changes the status of a task (e.g., Pending, InProgress, Completed, Cancelled)."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Task status updated successfully", typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid status", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Task not found", typeof(Error))]
    public async Task<IActionResult> ChangeTaskStatus(
        Guid id,
        [FromBody] ChangeTaskStatusCommand command,
        CancellationToken cancellationToken)
    {
        var updateCommand = command with { TaskId = id };
        var result = await _sender.Send(updateCommand, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Error?.Type switch
        {
            ErrorType.Validation => BadRequest(result.Error),
            ErrorType.Unauthorized => Unauthorized(result.Error),
            ErrorType.Conflict => Conflict(result.Error),
            ErrorType.NotFound => NotFound(result.Error),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Error)
        };
    }
}

