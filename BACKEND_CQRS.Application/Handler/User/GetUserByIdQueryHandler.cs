using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.User;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;

namespace BACKEND_CQRS.Application.Handler.User
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ApiResponse<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUserByIdQueryHandler(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            if (request.UserId <= 0)
            {
                return ApiResponse<UserDto>.Fail("Invalid user ID. User ID must be greater than 0.");
            }

            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                return ApiResponse<UserDto>.Fail($"User with ID {request.UserId} not found.");
            }

            var userDto = _mapper.Map<UserDto>(user);

            return ApiResponse<UserDto>.Success(userDto);
        }
    }
}
