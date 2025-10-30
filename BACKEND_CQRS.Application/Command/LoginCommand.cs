using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;

namespace BACKEND_CQRS.Application.Command
{
    public class LoginCommand : IRequest<ApiResponse<LoginResponseDto>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
