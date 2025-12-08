using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Repositories;
using Domain.Projects;
using Domain.Projects.Errors;
using SharedKernel;

namespace Application.Projects.Commands.DeleteProject;

internal sealed class DeleteProjectCommandHandler : ICommandHandler<DeleteProjectCommand, Guid>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IApplicationDbContext _context;

    public DeleteProjectCommandHandler(IProjectRepository projectRepository, IApplicationDbContext context)
    {
        _projectRepository = projectRepository;
        _context = context;
    }

    public async Task<Result<Guid>> Handle(DeleteProjectCommand command, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);

        if (project is null)
        {
            return ProjectErrors.NotFound(command.ProjectId);
        }

        _projectRepository.Remove(project);
        await _context.SaveChangesAsync(cancellationToken);

        return command.ProjectId;
    }
}

