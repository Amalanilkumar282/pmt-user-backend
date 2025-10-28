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
                // ✅ Entity → DTO mapping
                CreateMap<Teams, TeamDto>()
                    .ForMember(dest => dest.ProjectName,
                        opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : string.Empty))
                    .ForMember(dest => dest.LeadName,
                        opt => opt.MapFrom(src => src.Lead != null && src.Lead.User != null ? src.Lead.User.Name : string.Empty))
                   
                    .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.MemberCount))
                    .ForMember(dest => dest.ActiveSprintCount, opt => opt.MapFrom(src => src.ActiveSprintCount))
                    .ReverseMap();

                // ✅ DTO → Entity mapping
                CreateMap<CreateTeamDto, Teams>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
                    .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true))
                    .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                    .ForMember(dest => dest.Project, opt => opt.Ignore())
                    .ForMember(dest => dest.Lead, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedByMember, opt => opt.Ignore())
                    .ForMember(dest => dest.UpdatedByMember, opt => opt.Ignore())
                    .ForMember(dest => dest.MemberCount, opt => opt.Ignore())
                    .ForMember(dest => dest.ActiveSprintCount, opt => opt.Ignore());
            }
        }
    

}
