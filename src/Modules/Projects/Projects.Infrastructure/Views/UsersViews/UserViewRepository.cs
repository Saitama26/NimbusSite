using Microsoft.EntityFrameworkCore;
using Projects.Application.Abstractions.Views;

namespace Projects.Infrastructure.Views.UsersViews;

/// <summary>
/// Реализация репозитория чтения пользователей через Database View.
/// </summary>
internal sealed class UserViewRepository : IUserViewRepository
{
    private readonly ProjectsDbContext _dbContext;

    public UserViewRepository(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserViewDto?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserViews
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new UserViewDto
            {
                Id = x.Id,
                Email = x.Email,
                Name = x.Name,
                Status = x.Status,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserViewDto>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var ids = userIds?.ToList() ?? new List<Guid>();
        if (ids.Count == 0)
        {
            return Array.Empty<UserViewDto>();
        }

        return await _dbContext.UserViews
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new UserViewDto
            {
                Id = x.Id,
                Email = x.Email,
                Name = x.Name,
                Status = x.Status,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserViews
            .AsNoTracking()
            .AnyAsync(x => x.Id == userId, cancellationToken);
    }

}

