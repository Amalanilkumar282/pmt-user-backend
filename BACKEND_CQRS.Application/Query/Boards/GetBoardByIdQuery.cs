using BACKEND_CQRS.Application.Dto;
using MediatR;

namespace BACKEND_CQRS.Application.Query.Boards
{
    /// <summary>
    /// Query to get a single board by its ID with all related data
    /// </summary>
    public class GetBoardByIdQuery : IRequest<BoardWithColumnsDto?>
    {
        public int BoardId { get; }
        public bool IncludeInactive { get; }

        public GetBoardByIdQuery(int boardId, bool includeInactive = false)
        {
            BoardId = boardId;
            IncludeInactive = includeInactive;
        }
    }
}
