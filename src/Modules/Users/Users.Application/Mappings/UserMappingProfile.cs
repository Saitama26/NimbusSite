using AutoMapper;
using Users.Application.DTOs;
using Users.Domain.Entities;

namespace Users.Application.Mappings;

/// <summary>
/// AutoMapper профиль для маппинга User сущностей в DTO
/// </summary>
public sealed class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<User, UserListItemDto>();
    }
}

