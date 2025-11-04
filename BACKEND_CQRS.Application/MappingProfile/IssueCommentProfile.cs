using AutoMapper;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Domain.Entities;
using System.Linq;

namespace BACKEND_CQRS.Application.MappingProfile
{
    public class IssueCommentProfile : Profile
    {
        public IssueCommentProfile()
        {
            // Map from CreateIssueCommentCommand to IssueComment entity
            CreateMap<CreateIssueCommentCommand, IssueComment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.AuthorId))
                .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.AuthorId))
                .ForMember(dest => dest.MentionId, opt => opt.MapFrom(src => src.AuthorId));

            // Map from IssueComment entity to IssueCommentDto
            CreateMap<IssueComment, IssueCommentDto>()
                .ForMember(dest => dest.IssueId, opt => opt.MapFrom(src => src.IssueId.HasValue ? src.IssueId.Value : Guid.Empty))
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author != null ? src.Author.Name : null))
                .ForMember(dest => dest.AuthorAvatarUrl, opt => opt.MapFrom(src => src.Author != null ? src.Author.AvatarUrl : null))
                .ForMember(dest => dest.Mentions, opt => opt.MapFrom(src => src.Mentions));

            // Map from IssueComment entity to CreateIssueCommentDto
            CreateMap<IssueComment, CreateIssueCommentDto>()
                .ForMember(dest => dest.IssueId, opt => opt.MapFrom(src => src.IssueId.HasValue ? src.IssueId.Value : Guid.Empty));

            // Map from Mention entity to MentionDto
            CreateMap<Mention, MentionDto>()
                .ForMember(dest => dest.MentionUserId, opt => opt.MapFrom(src => src.MentionUserId.HasValue ? src.MentionUserId.Value : 0))
                .ForMember(dest => dest.MentionUserName, opt => opt.MapFrom(src => src.MentionedUser != null ? src.MentionedUser.Name : null))
                .ForMember(dest => dest.MentionUserEmail, opt => opt.MapFrom(src => src.MentionedUser != null ? src.MentionedUser.Email : null));
        }
    }
}
