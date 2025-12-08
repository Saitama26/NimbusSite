using Application.AccessPermissions.Commands.GrantPermission;
using Application.AccessPermissions.Commands.RevokePermission;
using Application.AccessPermissions.Queries.GetPermissions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Api.Controllers;

[ApiController]
[Route("api/permissions")]
[Authorize]
public sealed class AccessPermissionsController : ControllerBase
{
    private readonly ISender _sender;

    public AccessPermissionsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all permissions, optionally filtered by user, project, or tenant.
    /// </summary>
    /// <param name="userId">Optional user ID to filter permissions.</param>
    /// <param name="projectId">Optional project ID to filter permissions.</param>
    /// <param name="tenantId">Optional tenant ID to filter permissions.</param>
    /// <param name="includeRevoked">Whether to include revoked permissions. Default is false.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of permissions.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all permissions",
        Description = "Retrieves a list of all permissions. Can be filtered by UserId, ProjectId, or TenantId. By default, revoked permissions are excluded."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Permissions retrieved successfully", typeof(IReadOnlyList<PermissionResponse>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized", typeof(Error))]
    public async Task<IActionResult> GetPermissions(
        [FromQuery] Guid? userId,
        [FromQuery] Guid? projectId,
        [FromQuery] Guid? tenantId,
        [FromQuery] bool includeRevoked = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPermissionsQuery(userId, projectId, tenantId, includeRevoked);
        var result = await _sender.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Grant a permission to a user for a project.
    /// </summary>
    /// <param name="command">Permission grant details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the newly created permission.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Grant permission",
        Description = "Grants a specific role/permission to a user for a project within a tenant."
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "Permission granted successfully", typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input or permission already exists", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Tenant, user, or project not found", typeof(Error))]
    public async Task<IActionResult> GrantPermission(
        [FromBody] GrantPermissionCommand command,
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
    /// Revoke a permission from a user for a project.
    /// </summary>
    /// <param name="command">Permission revocation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the revoked permission.</returns>
    [HttpDelete]
    [SwaggerOperation(
        Summary = "Revoke permission",
        Description = "Revokes a permission from a user for a project. The permission is marked as revoked but not deleted."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Permission revoked successfully", typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Permission not found", typeof(Error))]
    public async Task<IActionResult> RevokePermission(
        [FromBody] RevokePermissionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
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

