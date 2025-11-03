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

            CreateMap<Issue, IssueDto>()
               .ForMember(dest => dest.IssueType, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status != null ? src.Status.StatusName : null))
                .ForMember(dest => dest.AssigneeName, opt => opt.MapFrom(src => src.Assignee != null ? src.Assignee.Name : null))
                .ForMember(dest => dest.SprintName, opt => opt.MapFrom(src => src.Sprint.Name))
                .ForMember(dest => dest.EpicName, opt => opt.MapFrom(src => src.Epic.Title)); // ?? Assignee’s name
            // Map from EditIssueCommand to Issue entity
            CreateMap<EditIssueCommand, Issue>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.IssueType));
        }
    }
}
