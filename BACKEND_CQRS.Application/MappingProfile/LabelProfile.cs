using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Application.Command;

namespace BACKEND_CQRS.Application.MappingProfile
{
    public class LabelProfile : Profile
    {
        public LabelProfile()
        {
            CreateMap<Label, LabelDto>();
            CreateMap<LabelDto, Label>();
            CreateMap<CreateLabelCommand, Label>();
        }
    }
}
