using AutoMapper;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Domain.Entities;

namespace BACKEND_CQRS.Application.MappingProfile
{
    public class SprintProfile : Profile
    {
        public SprintProfile()
        {
            CreateMap<Sprint, CreateSprintDto>()
                .ForMember(dest => dest.SprintName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TeamAssigned, opt => opt.MapFrom(src => src.TeamId))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<CreateSprintCommand, Sprint>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.SprintName))
                .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.TeamAssigned))
                .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
        }
    }
}
