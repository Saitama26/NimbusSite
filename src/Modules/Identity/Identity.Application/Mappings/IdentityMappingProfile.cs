using AutoMapper;
using Identity.Application.DTOs;
using Identity.Domain.Entities;

namespace Identity.Application.Mappings;

/// <summary>
/// AutoMapper профиль для маппинга Identity сущностей в DTO
/// </summary>
public sealed class IdentityMappingProfile : Profile
{
    public IdentityMappingProfile()
    {
        CreateMap<Session, SessionDto>();
    }
}

