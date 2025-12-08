using Application.Abstractions.Messaging;

namespace Application.Projects.Commands.UpdateProject;

public sealed record UpdateProjectCommand(
    Guid ProjectId,
    string? Name = null,
    string? Description = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : ICommand<Guid> {}

