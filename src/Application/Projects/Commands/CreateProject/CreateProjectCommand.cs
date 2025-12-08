using Application.Abstractions.Messaging;

namespace Application.Projects.Commands.CreateProject;
    
public sealed record CreateProjectCommand(    
    Guid TenantId,
    string Name,
    string Description,
    Guid OwnerId,
    DateTime StartDate,
    DateTime? EndDate) : ICommand<Guid> { }