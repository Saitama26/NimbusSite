using AutoMapper;
using Tasks.Application.DTOs;
using Tasks.Domain.Entities;
using Tasks.Domain.Enums;
using TaskStatus = Tasks.Domain.Enums.TaskStatus;
using DomainTask = Tasks.Domain.Entities.Task;

namespace Tasks.Application.Mappings;

public sealed class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        CreateMap<DomainTask, TaskDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Priority, opt => opt.MapFrom(s => s.Priority.ToString()));

        CreateMap<DomainTask, TaskListItemDto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Priority, opt => opt.MapFrom(s => s.Priority.ToString()));
    }
}

