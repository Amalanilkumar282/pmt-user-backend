using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Domain.Entities;
using AutoMapper;

namespace BACKEND_CQRS.Application.MappingProfile
{
    public class ProjectProfile : Profile
    {
        public ProjectProfile()
        {
            // Mapping from entity to DTO
            CreateMap<Projects, ProjectDto>();
        }
    }
}
