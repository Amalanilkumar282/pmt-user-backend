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
    public class EpicProfile : Profile
    {
        public EpicProfile()
        {
            CreateMap<Epic, EpicDto>()

                .ForMember(dest => dest.AssigneeName, opt => opt.MapFrom(src => src.Assignee != null ? src.Assignee.Name : string.Empty))
                .ForMember(dest => dest.ReporterName, opt => opt.MapFrom(src => src.Reporter != null ? src.Reporter.Name : string.Empty))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.Labels ?? new List<string>()));
        }
    }
}
