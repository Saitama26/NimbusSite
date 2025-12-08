using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Repositories;
using Domain.Projects;
using Domain.Projects.Errors;
using Domain.Projects.Events;
using Domain.Projects.ValueObjects;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Projects.Commands.UpdateProject;

internal sealed class UpdateProjectCommandHandler : ICommandHandler<UpdateProjectCommand, Guid>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<UpdateProjectCommandHandler> _logger;

    public UpdateProjectCommandHandler(
        IProjectRepository projectRepository,
        IApplicationDbContext context,
        ILogger<UpdateProjectCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(UpdateProjectCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating project {ProjectId}",
            command.ProjectId);

        var project = await _projectRepository.GetByIdAsync(command.ProjectId, cancellationToken);

        if (project is null)
        {
            _logger.LogWarning(
                "Project {ProjectId} not found",
                command.ProjectId);
            return ProjectErrors.NotFound(command.ProjectId);
        }

        if (project.Status == ProjectStatus.Completed || project.Status == ProjectStatus.Archived)
        {
            _logger.LogWarning(
                "Attempted to update project {ProjectId} with status {Status}",
                command.ProjectId,
                project.Status);
            return ProjectErrors.AlreadyClosed();
        }

        if (command.StartDate.HasValue && command.EndDate.HasValue && command.EndDate.Value < command.StartDate.Value)
        {
            return ProjectErrors.InvalidDateRange();
        }

        bool hasChanges = false;

        if (command.Name is not null && project.Name != command.Name)
        {
            project.Name = command.Name;
            hasChanges = true;
        }

        if (command.Description is not null && project.Description != command.Description)
        {
            project.Description = command.Description;
            hasChanges = true;
        }

        if (command.StartDate.HasValue && project.StartDate != command.StartDate.Value)
        {
            project.StartDate = command.StartDate.Value;
            hasChanges = true;
        }

        if (command.EndDate.HasValue && project.EndDate != command.EndDate.Value)
        {
            project.EndDate = command.EndDate.Value;
            hasChanges = true;
        }

        if (hasChanges)
        {
            project.AddEvent(new ProjectUpdatedEvent(project.Id, project.Name));
            _logger.LogInformation(
                "Project {ProjectId} updated successfully",
                command.ProjectId);
        }
        else
        {
            _logger.LogInformation(
                "Project {ProjectId} has no changes to update",
                command.ProjectId);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}