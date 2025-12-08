using Application.Authentication.Commands.RefreshToken;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Web.Api.Endpoints;

namespace Web.Api.Endpoints.Auth;

public sealed class Refresh : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/refresh", async (RefreshTokenCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(statusCode: MapStatus(result.Error!.Type), detail: result.Error.Description, title: result.Error.Code);
        })
        .AllowAnonymous()
        .WithTags("Auth")
        .WithSummary("Refresh token")
        .WithDescription("Exchanges a valid refresh token for a new access/refresh pair.");
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

