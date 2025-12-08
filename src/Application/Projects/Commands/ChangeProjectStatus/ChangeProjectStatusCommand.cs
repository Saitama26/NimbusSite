using Application.Abstractions.Messaging;
using Domain.Projects.ValueObjects;

namespace Application.Projects.Commands.ChangeProjectStatus;

public sealed record ChangeProjectStatusCommand(
    Guid ProjectId,
    ProjectStatus NewStatus
) : ICommand<Guid> {}

