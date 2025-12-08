using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Repositories;
using Domain.Projects;
using Domain.Projects.Errors;
using Domain.Projects.Events;
using Domain.Projects.ValueObjects;
using SharedKernel;

namespace Application.Projects.Commands.ChangeProjectStatus;

internal sealed class ChangeProjectStatusCommandHandler : ICommandHandler<ChangeProjectStatusCommand, Guid>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IApplicationDbContext _context;

    public ChangeProjectStatusCommandHandler(IProjectRepository projectRepository, IApplicationDbContext context)
    {
        _projectRepository = projectRepository;
        _context = context;
    }

    public async Task<Result<Guid>> Handle(ChangeProjectStatusCommand command, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);

        if (project is null)
        {
            return ProjectErrors.NotFound(command.ProjectId);
        }

        if (project.Status == command.NewStatus)
        {
            return project.Id; // Статус уже установлен
        }

        var oldStatus = project.Status;
        project.Status = command.NewStatus;
        project.AddEvent(new ProjectStatusChangedEvent(project.Id, oldStatus, command.NewStatus));

        await _context.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}

