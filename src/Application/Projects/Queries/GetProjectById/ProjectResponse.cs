using Domain.Projects.ValueObjects;

namespace Application.Projects.Queries.GetProjectById;

public sealed record ProjectResponse(
    Guid ProjectId,
    Guid TenantId,
    string Name,
    string Description,
    Guid OwnerId,
    DateTime StartDate,
    DateTime? EndDate,
    ProjectStatus Status,
    DateTime CreatedAt,
    IReadOnlyList<Guid> MemberIds) { }

