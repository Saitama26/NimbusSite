using Application.Authentication.Commands.Logout;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Web.Api.Endpoints;

namespace Web.Api.Endpoints.Auth;

public sealed class Logout : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/auth/logout", async (LogoutCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok()
                : Results.Problem(statusCode: MapStatus(result.Error!.Type), detail: result.Error.Description, title: result.Error.Code);
        })
        .RequireAuthorization()
        .WithTags("Auth")
        .WithSummary("Logout")
        .WithDescription("Revokes the provided refresh token.");
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

