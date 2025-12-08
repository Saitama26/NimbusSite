using Application.Abstractions.Messaging;

namespace Application.Projects.Commands.DeleteProject;

public sealed record DeleteProjectCommand(
    Guid ProjectId
) : ICommand<Guid> {}

