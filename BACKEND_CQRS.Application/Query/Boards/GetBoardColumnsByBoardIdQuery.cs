using BACKEND_CQRS.Application.Dto;
using MediatR;

namespace BACKEND_CQRS.Application.Query.Boards
{
    public class GetBoardColumnsByBoardIdQuery : IRequest<List<BoardColumnDto>>
    {
        public int BoardId { get; }

        public GetBoardColumnsByBoardIdQuery(int boardId)
        {
            BoardId = boardId;
        }
    }
}
