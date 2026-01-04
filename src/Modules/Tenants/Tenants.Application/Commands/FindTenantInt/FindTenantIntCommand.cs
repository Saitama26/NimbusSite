using Common.Application.Abstractions.Messaging;
using Common.Application.Abstractions.Tenancy;
using Common.Domain.Results;
using Microsoft.EntityFrameworkCore;
using Tenants.Application.Abstractions;

namespace Tenants.Application.Commands.FindTenantInt;

/// <summary>
/// Команда для получения числового идентификатора тенанта по его имени
/// Используется как API Function для ITenancyDomain
/// </summary>
internal sealed record FindTenantIntCommand(string TenantName) : ICommand<int>;

/// <summary>
/// Обработчик команды FindTenantInt
/// </summary>
internal sealed class FindTenantIntCommandHandler : ICommandHandler<FindTenantIntCommand, int>
{
    private readonly ITenantsDbContext _dbContext;

    public FindTenantIntCommandHandler(ITenantsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<int>> Handle(FindTenantIntCommand command, CancellationToken cancellationToken)
    {
        var tenant = await _dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Name == command.TenantName, cancellationToken);

        if (tenant == null)
        {
            return Result<int>.Failure(Error.NotFound("Tenant.NotFound", $"Tenant with name '{command.TenantName}' not found"));
        }

        return Result<int>.Success(tenant.TenantInt);
    }
}

