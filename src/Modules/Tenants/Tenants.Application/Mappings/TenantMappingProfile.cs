using AutoMapper;
using Tenants.Application.DTOs;
using Tenants.Domain.Entities;

namespace Tenants.Application.Mappings;

/// <summary>
/// AutoMapper профиль для маппинга Tenant сущностей в DTO
/// </summary>
public sealed class TenantMappingProfile : Profile
{
    public TenantMappingProfile()
    {
        CreateMap<Tenant, TenantDto>();
        CreateMap<Tenant, TenantListItemDto>();
    }
}

