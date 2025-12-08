using Application.Abstractions.Messaging;

namespace Application.Projects.Commands.AddProjectMember;

public sealed record AddProjectMemberCommand(
    Guid ProjectId,
    Guid UserId
) : ICommand<Guid> {}

