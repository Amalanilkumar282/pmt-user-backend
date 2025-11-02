using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Domain.Entities;
using AutoMapper;

namespace BACKEND_CQRS.Application.MappingProfile
{
    public class ProjectProfile : Profile
    {
        public ProjectProfile()
        {
            // Entity -> DTO
            CreateMap<Projects, ProjectDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CustomerOrgName, opt => opt.MapFrom(src => src.CustomerOrgName))
                .ForMember(dest => dest.CustomerDomainUrl, opt => opt.MapFrom(src => src.CustomerDomainUrl))
                .ForMember(dest => dest.CustomerDescription, opt => opt.MapFrom(src => src.CustomerDescription))
                .ForMember(dest => dest.PocEmail, opt => opt.MapFrom(src => src.PocEmail))
                .ForMember(dest => dest.PocPhone, opt => opt.MapFrom(src => src.PocPhone))
                .ForMember(dest => dest.ProjectManagerName, opt => opt.MapFrom(src => src.ProjectManager != null ? src.ProjectManager.Name : null))
                .ForMember(dest => dest.DeliveryUnitName, opt => opt.MapFrom(src => src.DeliveryUnit != null ? src.DeliveryUnit.Name : null))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status != null ? src.Status.Name : null))
                .ReverseMap();
        }
    }
}
