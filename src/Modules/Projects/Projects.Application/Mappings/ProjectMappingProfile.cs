using AutoMapper;
using Projects.Application.DTOs;
using Projects.Domain.Entities;
using Projects.Domain.Enums;

namespace Projects.Application.Mappings;

public sealed class ProjectMappingProfile : Profile
{
    public ProjectMappingProfile()
    {
        CreateMap<Project, ProjectDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<Project, ProjectListItemDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));
    }
}

