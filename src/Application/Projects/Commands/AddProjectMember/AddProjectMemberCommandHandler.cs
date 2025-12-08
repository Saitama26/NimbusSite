using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Repositories;
using Domain.Projects;
using Domain.Projects.Errors;
using Domain.Projects.Events;
using Domain.Users;
using Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Projects.Commands.AddProjectMember;

internal sealed class AddProjectMemberCommandHandler : ICommandHandler<AddProjectMemberCommand, Guid>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IApplicationDbContext _context;

    public AddProjectMemberCommandHandler(
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IApplicationDbContext context)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _context = context;
    }

    public async Task<Result<Guid>> Handle(AddProjectMemberCommand command, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);

        if (project is null)
        {
            return ProjectErrors.NotFound(command.ProjectId);
        }

        var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user is null)
        {
            return UserErrors.NotFound(command.UserId);
        }

        // Проверяем, не является ли пользователь уже участником проекта
        var isMember = await _context.Projects
            .Where(p => p.Id == command.ProjectId)
            .SelectMany(p => p.Members)
            .AnyAsync(m => m.Id == command.UserId, cancellationToken);

        if (isMember)
        {
            return project.Id; // Уже является участником
        }

        project.Members.Add(user);
        project.AddEvent(new ProjectMemberAddedEvent(project.Id, user.Id));

        await _context.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}

