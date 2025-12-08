using Domain.Common.Interfaces;
using Domain.Projects;
using Domain.Users;
using SharedKernel;

namespace Domain.Tenants;

public class Tenant : DomainEventEntity, IBaseEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Name { get; set; }
    public string ConnectionString { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}