using AutoMapper;
using AccessPermissions.Application.DTOs;
using AccessPermissions.Domain.Entities;

namespace AccessPermissions.Application.Mappings;

public sealed class AccessPermissionMappingProfile : Profile
{
    public AccessPermissionMappingProfile()
    {
        CreateMap<AccessPermission, AccessPermissionDto>();
    }
}

