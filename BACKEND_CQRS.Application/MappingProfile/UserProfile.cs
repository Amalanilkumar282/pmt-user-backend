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
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // Entity -> DTO
            CreateMap<Users, UserDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ?? false))
                .ForMember(dest => dest.IsSuperAdmin, opt => opt.MapFrom(src => src.IsSuperAdmin ?? false));

            // DTO -> Entity (explicit to handle nullable booleans and avoid overwriting sensitive fields)
            CreateMap<UserDto, Users>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.IsSuperAdmin, opt => opt.MapFrom(src => src.IsSuperAdmin))
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()); // don't map password from DTO
        }
    }
}
