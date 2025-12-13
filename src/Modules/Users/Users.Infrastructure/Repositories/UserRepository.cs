using Microsoft.EntityFrameworkCore;
using Users.Application.Abstractions;
using Users.Domain.Entities;
using Users.Infrastructure;

namespace Users.Infrastructure.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly UsersDbContext _dbContext;

    public UserRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailLower = email.ToLowerInvariant().Trim();
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == emailLower, cancellationToken);
    }

    public Task<IQueryable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // IQueryable остается валидным в рамках жизненного цикла DbContext (scoped).
        return Task.FromResult(_dbContext.Users.AsNoTracking().AsQueryable());
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailLower = email.ToLowerInvariant().Trim();
        return await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(x => x.Email == emailLower, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        // Нормализуем email перед сохранением
        user.Email = user.Email.ToLowerInvariant().Trim();
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        // Нормализуем email перед сохранением
        user.Email = user.Email.ToLowerInvariant().Trim();
        _dbContext.Users.Update(user);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        _dbContext.Users.Remove(user);
        return Task.CompletedTask;
    }
}

