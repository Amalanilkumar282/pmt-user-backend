using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;

namespace BACKEND_CQRS.Application.Query.Statuses
{
    /// <summary>
    /// Query to get a single status by its ID
    /// </summary>
    public class GetStatusByIdQuery : IRequest<ApiResponse<StatusDto>>
    {
        public int StatusId { get; }

        public GetStatusByIdQuery(int statusId)
        {
            StatusId = statusId;
        }
    }
}
