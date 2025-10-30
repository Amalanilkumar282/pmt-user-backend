using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Domain.Entities;

namespace BACKEND_CQRS.Application.MappingProfile
{
    public class ChannelProfile : Profile
    {
        public ChannelProfile()
        {
            // Entity -> DTO
            CreateMap<Channel, ChannelDto>();

            // DTO -> Entity
            CreateMap<ChannelDto, Channel>();
        }
    }
}