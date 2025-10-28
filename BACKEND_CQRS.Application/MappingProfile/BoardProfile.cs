using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Domain.Entities;

namespace BACKEND_CQRS.Application.MappingProfile
{
    public class BoardProfile : Profile
    {
        public BoardProfile()
        {
            // Map BoardColumn Entity ? BoardColumnDto
            CreateMap<BoardColumn, BoardColumnDto>()
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status != null ? src.Status.StatusName : null));

            // Map Board Entity ? BoardWithColumnsDto
            CreateMap<Board, BoardWithColumnsDto>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : null))
                .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team != null ? src.Team.Name : null))
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.Name : null))
                .ForMember(dest => dest.UpdatedByName, opt => opt.MapFrom(src => src.Updater != null ? src.Updater.Name : null))
                .ForMember(dest => dest.Columns, opt => opt.MapFrom(src => src.BoardColumns));
        }
    }
}
