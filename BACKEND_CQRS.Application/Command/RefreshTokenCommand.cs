using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;

namespace BACKEND_CQRS.Application.Command
{
    public class RefreshTokenCommand : IRequest<ApiResponse<LoginResponseDto>>
    {
        public string RefreshToken { get; set; }
    }
}
