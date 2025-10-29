using BACKEND_CQRS.Application.Dto;
using MediatR;

namespace BACKEND_CQRS.Application.Query
{
    public class GetBoardsByProjectIdQuery : IRequest<List<BoardWithColumnsDto>>
    {
        public Guid ProjectId { get; }

        public GetBoardsByProjectIdQuery(Guid projectId)
        {
            ProjectId = projectId;
        }
    }
}
