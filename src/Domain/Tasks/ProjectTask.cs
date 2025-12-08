using Domain.Common.Interfaces;
using Domain.Projects;
using Domain.Tasks.ValueObjects;
using Domain.Users;
using SharedKernel;

namespace Domain.Tasks;

public class ProjectTask : DomainEventEntity, IBaseEntity
{
    public Guid Id { get ; set ; }
    public Guid TenantId { get ; set ; }
    public DateTime CreatedAt { get ; set ; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime? DueDate { get; set; }

    public Status Status { get; set; } = Status.New;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public Guid ProjectId { get; set; }
    public Project Project { get; set; }

    public Guid AssignedUserId { get; set; }
    public User AssignedUser { get; set; }
}
