using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Projects;
using Domain.Projects.Errors;
using Domain.Projects.Events;
using Domain.Projects.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Projects.Commands.CreateProject;

internal sealed class CreateProjectCommandHandler : ICommandHandler<CreateProjectCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateProjectCommandHandler> _logger;

    public CreateProjectCommandHandler(
        IApplicationDbContext context,
        ILogger<CreateProjectCommandHandler> logger) 
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating project with name '{ProjectName}' for tenant {TenantId}",
            request.Name,
            request.TenantId);

        // Проверяем уникальность имени проекта в рамках тенанта
        var projectExists = await _context.Projects
            .AnyAsync(
                p => p.TenantId == request.TenantId && p.Name == request.Name,
                cancellationToken);

        if (projectExists)
        {
            _logger.LogWarning(
                "Project with name '{ProjectName}' already exists for tenant {TenantId}",
                request.Name,
                request.TenantId);
            return ProjectErrors.AlreadyExists(request.Name);
        }

        var projectId = Guid.NewGuid();
        var project = new Project() 
        {
            Id = projectId,
            TenantId = request.TenantId,
            Name = request.Name,
            Description = request.Description,
            OwnerId = request.OwnerId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAt = DateTime.UtcNow,
            Status = ProjectStatus.Active
        };

        project.AddEvent(new ProjectCreatedEvent(project.Id, project.Name));
        _context.Projects.Add(project);
        
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Project {ProjectId} with name '{ProjectName}' created successfully for tenant {TenantId}",
            projectId,
            request.Name,
            request.TenantId);

        return project.Id;
    }
}