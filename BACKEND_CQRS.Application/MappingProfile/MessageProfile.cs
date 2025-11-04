using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Domain.Entities;

namespace BACKEND_CQRS.Application.MappingProfile
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.MentionedUserName, opt => opt.MapFrom(src => src.MentionedUser != null ? src.MentionedUser.Name : null))
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.Name : null));
        }
    }
}