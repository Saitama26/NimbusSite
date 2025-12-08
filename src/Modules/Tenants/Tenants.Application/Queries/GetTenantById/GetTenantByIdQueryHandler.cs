using AutoMapper;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Tenants.Application.Abstractions;
using Tenants.Application.DTOs;
using Tenants.Application.Queries.GetTenantById;
using Tenants.Domain.Errors;

namespace Tenants.Application.Queries.GetTenantById;

/// <summary>
/// Обработчик запроса получения тенанта по ID
/// </summary>
internal sealed class GetTenantByIdQueryHandler : IQueryHandler<GetTenantByIdQuery, TenantDto>
{
    private readonly ITenantRepository _repository;
    private readonly IMapper _mapper;

    public GetTenantByIdQueryHandler(ITenantRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<TenantDto>> Handle(GetTenantByIdQuery query, CancellationToken cancellationToken)
    {
        // Найти тенанта
        var tenant = await _repository.GetByIdAsync(query.TenantId, cancellationToken);
        if (tenant == null)
        {
            return Result<TenantDto>.Failure(TenantErrors.NotFound(query.TenantId));
        }

        // Маппинг в DTO
        var dto = _mapper.Map<TenantDto>(tenant);

        return Result<TenantDto>.Success(dto);
    }
}

