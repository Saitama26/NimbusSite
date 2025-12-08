using Domain.Common.Interfaces;
using Domain.Tasks;
using Domain.Users;
using Domain.Projects.ValueObjects;
using SharedKernel;

namespace Domain.Projects;

public class Project : DomainEventEntity, IBaseEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ProjectStatus Status { get; set; }

    public Guid OwnerId { get; set; }
    public User Owner { get; set; }

    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    public ICollection<User> Members { get; set; } = new List<User>();
}
