using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace BACKEND_CQRS.Application.Command
{
    /// <summary>
    /// Command to delete a board column
    /// </summary>
    public class DeleteBoardColumnCommand : IRequest<ApiResponse<DeleteBoardColumnResponseDto>>
    {
        [Required(ErrorMessage = "Column ID is required")]
        public Guid ColumnId { get; set; }

        [Required(ErrorMessage = "Board ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Board ID must be greater than 0")]
        public int BoardId { get; set; }

        /// <summary>
        /// Optional: User ID who is deleting the column (for audit purposes)
        /// </summary>
        public int? DeletedBy { get; set; }

        public DeleteBoardColumnCommand()
        {
        }

        public DeleteBoardColumnCommand(Guid columnId, int boardId, int? deletedBy = null)
        {
            ColumnId = columnId;
            BoardId = boardId;
            DeletedBy = deletedBy;
        }
    }
}
