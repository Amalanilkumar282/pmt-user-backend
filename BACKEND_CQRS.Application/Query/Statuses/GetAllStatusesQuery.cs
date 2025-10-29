using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;

namespace BACKEND_CQRS.Application.Query.Statuses
{
    public class GetAllStatusesQuery : IRequest<ApiResponse<List<StatusDto>>>
    {
    }
}
