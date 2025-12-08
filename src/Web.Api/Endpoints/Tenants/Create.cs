using Application.Tenants.Commands.CreateTenant;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Web.Api.Endpoints;

namespace Web.Api.Endpoints.Tenants;

public sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/tenants", async (CreateTenantCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(statusCode: MapStatus(result.Error!.Type), detail: result.Error.Description, title: result.Error.Code);
        })
        .AllowAnonymous()
        .WithTags("Tenants")
        .WithSummary("Create tenant")
        .WithDescription("Creates a tenant with its own connection string. Use returned tenantId when registering users.");
    }

    private static int MapStatus(SharedKernel.ErrorType type) => type switch
    {
        SharedKernel.ErrorType.Validation => StatusCodes.Status400BadRequest,
        SharedKernel.ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        SharedKernel.ErrorType.Conflict => StatusCodes.Status409Conflict,
        SharedKernel.ErrorType.NotFound => StatusCodes.Status404NotFound,
        _ => StatusCodes.Status500InternalServerError
    };
}

