using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Repositories;
using Domain.Projects;
using Domain.Projects.Errors;
using Domain.Projects.Events;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Projects.Commands.RemoveProjectMember;

internal sealed class RemoveProjectMemberCommandHandler : ICommandHandler<RemoveProjectMemberCommand, Guid>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IApplicationDbContext _context;

    public RemoveProjectMemberCommandHandler(IProjectRepository projectRepository, IApplicationDbContext context)
    {
        _projectRepository = projectRepository;
        _context = context;
    }

    public async Task<Result<Guid>> Handle(RemoveProjectMemberCommand command, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken);

        if (project is null)
        {
            return ProjectErrors.NotFound(command.ProjectId);
        }

        var member = project.Members.FirstOrDefault(m => m.Id == command.UserId);

        if (member is null)
        {
            return project.Id; // Не является участником
        }

        // Нельзя удалить владельца проекта
        if (project.OwnerId == command.UserId)
        {
            return ProjectErrors.NotFound(command.ProjectId); // Или можно создать отдельную ошибку
        }

        project.Members.Remove(member);
        project.AddEvent(new ProjectMemberRemovedEvent(project.Id, command.UserId));

        await _context.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}

