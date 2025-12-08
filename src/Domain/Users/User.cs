using Domain.Common.Interfaces;
using Domain.Projects;
using Domain.Tasks;
using Domain.Users.ValueObjects;
using SharedKernel;

namespace Domain.Users;

public sealed class User : DomainEventEntity, IBaseEntity
{
    public Guid Id { get ; set ; }
    public Guid TenantId { get ; set ; }
    public DateTime CreatedAt { get ; set ; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public bool IsActive { get; set; } = true;

    public UserRole Role { get; set; } = UserRole.Observer;

    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
}
