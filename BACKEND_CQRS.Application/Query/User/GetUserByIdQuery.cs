using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;

namespace BACKEND_CQRS.Application.Query.User
{
    public class GetUserByIdQuery : IRequest<ApiResponse<UserDto>>
    {
        public int UserId { get; set; }

        public GetUserByIdQuery(int userId)
        {
            UserId = userId;
        }
    }
}
