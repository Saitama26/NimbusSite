using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Tenants.Application.Abstractions;
using Tenants.Application.DTOs;
using Tenants.Application.Queries.GetTenants;

namespace Tenants.Application.Queries.GetTenants;

/// <summary>
/// Обработчик запроса получения списка тенантов
/// Возвращает IQueryable для OData пагинации, фильтрации и сортировки
/// </summary>
internal sealed class GetTenantsQueryHandler : IQueryHandler<GetTenantsQuery, IQueryable<TenantListItemDto>>
{
    private readonly ITenantRepository _repository;
    private readonly IMapper _mapper;

    public GetTenantsQueryHandler(ITenantRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IQueryable<TenantListItemDto>>> Handle(GetTenantsQuery query, CancellationToken cancellationToken)
    {
        // Получить IQueryable тенантов
        var tenantsQueryable = await _repository.GetAllAsync(cancellationToken);

        // Маппинг через AutoMapper в DTO (ProjectTo для IQueryable)
        var dtoQueryable = tenantsQueryable.ProjectTo<TenantListItemDto>(_mapper.ConfigurationProvider);

        return Result<IQueryable<TenantListItemDto>>.Success(dtoQueryable);
    }
}

