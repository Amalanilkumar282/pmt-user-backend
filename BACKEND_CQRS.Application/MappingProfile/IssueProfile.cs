using AutoMapper;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Domain.Entities;

namespace BACKEND_CQRS.Application.MappingProfile
{
    public class IssueProfile : Profile
    {
        public IssueProfile()
        {
            // Map from Issue entity to CreateIssueDto
            CreateMap<Issue, CreateIssueDto>()
                .ForMember(dest => dest.IssueType, opt => opt.MapFrom(src => src.Type));

            // Map from CreateIssueCommand to Issue entity
            CreateMap<CreateIssueCommand, Issue>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.IssueType));
        }
    }
}
