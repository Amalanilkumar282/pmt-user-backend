using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;

namespace BACKEND_CQRS.Application.Query.Statuses
{
    /// <summary>
    /// Query to get all statuses used in a specific project
    /// </summary>
    public class GetStatusesByProjectIdQuery : IRequest<ApiResponse<List<StatusDto>>>
    {
        public Guid ProjectId { get; }

        public GetStatusesByProjectIdQuery(Guid projectId)
        {
            ProjectId = projectId;
        }
    }
}
