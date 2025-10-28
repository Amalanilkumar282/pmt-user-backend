using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.MappingProfile
{
    public class TeamProfile : Profile
    {

        public TeamProfile()
        {
            // 🔹 Map from Team Entity → TeamDto
            CreateMap<Teams, TeamDto>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : string.Empty))
                .ForMember(dest => dest.LeadName, opt => opt.MapFrom(src => src.Lead != null ? src.Lead.Name : string.Empty))
                 .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.MemberCount))
                 .ForMember(dest => dest.ActiveSprintCount, opt => opt.MapFrom(src => src.ActiveSprintCount))
                .ReverseMap();
        }
    }
}
