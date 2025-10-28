using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Query.Users;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.User
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, ApiResponse<List<UserDto>>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetAllUsersQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            // Fetch all users from the repository
            var users = await _userRepository.GetAllAsync();

            if (users == null || !users.Any())
                return ApiResponse<List<UserDto>>.Fail("No users found.");

            // Map to DTOs
            var userDtos = _mapper.Map<List<UserDto>>(users);

            return ApiResponse<List<UserDto>>.Success(userDtos);
        }
    }
}
