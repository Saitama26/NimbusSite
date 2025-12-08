using Application.Abstractions.Messaging;

namespace Application.Projects.Commands.RemoveProjectMember;

public sealed record RemoveProjectMemberCommand(
    Guid ProjectId,
    Guid UserId
) : ICommand<Guid> {}

