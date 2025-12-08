using Domain.Common.Interfaces;
using Domain.Projects;
using Domain.Users;
using Domain.Users.ValueObjects;
using SharedKernel;

namespace Domain.Access;

public class AccessPermission : DomainEventEntity, IBaseEntity
{
    public Guid Id { get ; set ; }
    public Guid TenantId { get ; set ; }
    public DateTime CreatedAt { get ; set ; }
    public DateTime RevokedAt { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; }

    public UserRole Role { get; set; } // Owner, Manager, Contributor, Viewer
}
