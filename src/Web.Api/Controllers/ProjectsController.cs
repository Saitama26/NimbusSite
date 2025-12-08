using Application.Projects.Commands.AddProjectMember;
using Application.Projects.Commands.ChangeProjectStatus;
using Application.Projects.Commands.CreateProject;
using Application.Projects.Commands.DeleteProject;
using Application.Projects.Commands.RemoveProjectMember;
using Application.Projects.Commands.UpdateProject;
using Application.Projects.Queries.GetProjectById;
using Application.Projects.Queries.GetProjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Api.Controllers;

// Using aliases to resolve ambiguity between ProjectResponse types from different queries
using GetProjectsResponse = Application.Projects.Queries.GetProjects.ProjectResponse;
using GetProjectByIdResponse = Application.Projects.Queries.GetProjectById.ProjectResponse;

[ApiController]
[Route("api/projects")]
[Authorize]
public sealed class ProjectsController : ControllerBase
{
    private readonly ISender _sender;

    public ProjectsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all projects, optionally filtered by tenant or owner.
    /// </summary>
    /// <param name="tenantId">Optional tenant ID to filter projects.</param>
    /// <param name="ownerId">Optional owner ID to filter projects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of projects.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all projects",
        Description = "Retrieves a list of all projects. Can be filtered by TenantId or OwnerId."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Projects retrieved successfully", typeof(IReadOnlyList<GetProjectsResponse>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized", typeof(Error))]
    public async Task<IActionResult> GetProjects(
        [FromQuery] Guid? tenantId,
        [FromQuery] Guid? ownerId,
        CancellationToken cancellationToken)
    {
        var query = new GetProjectsQuery(tenantId, ownerId);
        var result = await _sender.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get a project by ID.
    /// </summary>
    /// <param name="id">The ID of the project to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The project details.</returns>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Get project by ID",
        Description = "Retrieves a single project by its unique identifier."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Project retrieved successfully", typeof(GetProjectByIdResponse))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Project not found", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized", typeof(Error))]
    public async Task<IActionResult> GetProjectById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetProjectByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Create a new project.
    /// </summary>
    /// <param name="command">Project creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the newly created project.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create a new project",
        Description = "Creates a new project with the provided details. Project name must be unique within the tenant."
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "Project created successfully", typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input or project name already exists", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Tenant or owner not found", typeof(Error))]
    public async Task<IActionResult> CreateProject(
        [FromBody] CreateProjectCommand command,
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
    /// Update an existing project.
    /// </summary>
    /// <param name="id">The ID of the project to update.</param>
    /// <param name="command">Project update details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the updated project.</returns>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        Summary = "Update project",
        Description = "Updates an existing project. Only provided fields will be updated."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Project updated successfully", typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Project not found", typeof(Error))]
    public async Task<IActionResult> UpdateProject(
        Guid id,
        [FromBody] UpdateProjectCommand command,
        CancellationToken cancellationToken)
    {
        var updateCommand = command with { ProjectId = id };
        var result = await _sender.Send(updateCommand, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Delete a project.
    /// </summary>
    /// <param name="id">The ID of the project to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the deleted project.</returns>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Delete project",
        Description = "Deletes a project and all associated tasks."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Project deleted successfully", typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Project not found", typeof(Error))]
    public async Task<IActionResult> DeleteProject(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProjectCommand(id);
        var result = await _sender.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Change the status of a project.
    /// </summary>
    /// <param name="id">The ID of the project.</param>
    /// <param name="command">Status change details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the updated project.</returns>
    [HttpPatch("{id:guid}/status")]
    [SwaggerOperation(
        Summary = "Change project status",
        Description = "Changes the status of a project (e.g., Active, Completed, Archived)."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Project status updated successfully", typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid status", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Project not found", typeof(Error))]
    public async Task<IActionResult> ChangeProjectStatus(
        Guid id,
        [FromBody] ChangeProjectStatusCommand command,
        CancellationToken cancellationToken)
    {
        var updateCommand = command with { ProjectId = id };
        var result = await _sender.Send(updateCommand, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Add a member to a project.
    /// </summary>
    /// <param name="id">The ID of the project.</param>
    /// <param name="command">Member addition details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the project.</returns>
    [HttpPost("{id:guid}/members")]
    [SwaggerOperation(
        Summary = "Add project member",
        Description = "Adds a user as a member to a project."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Member added successfully", typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input or member already exists", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Project or user not found", typeof(Error))]
    public async Task<IActionResult> AddProjectMember(
        Guid id,
        [FromBody] AddProjectMemberCommand command,
        CancellationToken cancellationToken)
    {
        var updateCommand = command with { ProjectId = id };
        var result = await _sender.Send(updateCommand, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Remove a member from a project.
    /// </summary>
    /// <param name="id">The ID of the project.</param>
    /// <param name="command">Member removal details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the project.</returns>
    [HttpDelete("{id:guid}/members")]
    [SwaggerOperation(
        Summary = "Remove project member",
        Description = "Removes a user from a project's member list."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Member removed successfully", typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Project or member not found", typeof(Error))]
    public async Task<IActionResult> RemoveProjectMember(
        Guid id,
        [FromBody] RemoveProjectMemberCommand command,
        CancellationToken cancellationToken)
    {
        var updateCommand = command with { ProjectId = id };
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

