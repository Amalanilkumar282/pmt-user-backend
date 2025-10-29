using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace BACKEND_CQRS.Application.Command
{
    /// <summary>
    /// Command to soft-delete a board
    /// </summary>
    public class DeleteBoardCommand : IRequest<ApiResponse<bool>>
    {
        [Required(ErrorMessage = "Board ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Board ID must be greater than 0")]
        public int BoardId { get; set; }

        /// <summary>
        /// Optional: User ID who is deleting the board (for audit purposes)
        /// </summary>
        public int? DeletedBy { get; set; }

        public DeleteBoardCommand()
        {
        }

        public DeleteBoardCommand(int boardId, int? deletedBy = null)
        {
            BoardId = boardId;
            DeletedBy = deletedBy;
        }
    }
}
