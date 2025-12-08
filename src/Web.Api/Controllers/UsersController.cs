using Application.Users.Queries.GetUserById;
using Application.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Api.Controllers;

// Using aliases to resolve ambiguity between UserResponse types from different queries
using GetUserByIdResponse = Application.Users.Queries.GetUserById.UserResponse;
using GetUsersResponse = Application.Users.Queries.GetUsers.UserResponse;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all users, optionally filtered by tenant.
    /// </summary>
    /// <param name="tenantId">Optional tenant ID to filter users.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of users.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all users",
        Description = "Retrieves a list of all users. Can be filtered by TenantId."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Users retrieved successfully", typeof(IReadOnlyList<GetUsersResponse>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized", typeof(Error))]
    public async Task<IActionResult> GetUsers(
        [FromQuery] Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var query = new GetUsersQuery(tenantId);
        var result = await _sender.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get a user by ID.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user details.</returns>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Get user by ID",
        Description = "Retrieves a single user by their unique identifier."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "User retrieved successfully", typeof(GetUserByIdResponse))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized", typeof(Error))]
    public async Task<IActionResult> GetUserById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);
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

