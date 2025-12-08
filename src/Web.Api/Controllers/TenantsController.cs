using Application.Tenants.Commands.CreateTenant;
using Application.Tenants.Commands.UpdateTenant;
using Application.Tenants.Queries.GetTenantById;
using Application.Tenants.Queries.GetTenants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Swashbuckle.AspNetCore.Annotations;

namespace Web.Api.Controllers;

// Using aliases to resolve ambiguity between TenantResponse types from different queries
using GetTenantsResponse = Application.Tenants.Queries.GetTenants.TenantResponse;
using GetTenantByIdResponse = Application.Tenants.Queries.GetTenantById.TenantResponse;

[ApiController]
[Route("api/tenants")]
public sealed class TenantsController : ControllerBase
{
    private readonly ISender _sender;

    public TenantsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get all tenants.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of tenants.</returns>
    [HttpGet]
    [Authorize]
    [SwaggerOperation(
        Summary = "Get all tenants",
        Description = "Retrieves a list of all tenants in the system."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Tenants retrieved successfully", typeof(IReadOnlyList<GetTenantsResponse>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized", typeof(Error))]
    public async Task<IActionResult> GetTenants(CancellationToken cancellationToken)
    {
        var query = new GetTenantsQuery();
        var result = await _sender.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Get a tenant by ID.
    /// </summary>
    /// <param name="id">The ID of the tenant to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tenant details.</returns>
    [HttpGet("{id:guid}")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Get tenant by ID",
        Description = "Retrieves a single tenant by its unique identifier."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Tenant retrieved successfully", typeof(GetTenantByIdResponse))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Tenant not found", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized", typeof(Error))]
    public async Task<IActionResult> GetTenantById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTenantByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>
    /// Create a new tenant.
    /// </summary>
    /// <remarks>
    /// Sample:
    /// POST /api/tenants
    /// {
    ///   "name": "Tenant A",
    ///   "connectionString": "Server=localhost;Port=3306;Database=TenantA;User=root;Password=sqlPassword123;CharSet=utf8mb4;"
    /// }
    /// </remarks>
    /// <param name="command">Tenant creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the newly created tenant.</returns>
    [AllowAnonymous]
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create tenant",
        Description = "Creates a tenant with its own connection string. Use returned tenantId when registering users."
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "Tenant created successfully", typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Validation error", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Tenant already exists", typeof(Error))]
    public async Task<IActionResult> Create(
        [FromBody] CreateTenantCommand command,
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
    /// Update an existing tenant.
    /// </summary>
    /// <param name="id">The ID of the tenant to update.</param>
    /// <param name="command">Tenant update details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the updated tenant.</returns>
    [HttpPut("{id:guid}")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Update tenant",
        Description = "Updates an existing tenant. Only provided fields will be updated."
    )]
    [SwaggerResponse(StatusCodes.Status200OK, "Tenant updated successfully", typeof(Guid))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input", typeof(Error))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Tenant not found", typeof(Error))]
    public async Task<IActionResult> UpdateTenant(
        Guid id,
        [FromBody] UpdateTenantCommand command,
        CancellationToken cancellationToken)
    {
        var updateCommand = command with { TenantId = id };
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

