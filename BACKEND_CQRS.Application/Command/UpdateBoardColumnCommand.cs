using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace BACKEND_CQRS.Application.Command
{
    /// <summary>
    /// Command to update a board column's properties including position, color, name, or status
    /// </summary>
    public class UpdateBoardColumnCommand : IRequest<ApiResponse<UpdateBoardColumnResponseDto>>
    {
        /// <summary>
        /// The ID of the board column to update (set from route parameter)
        /// </summary>
        public Guid ColumnId { get; set; }

        /// <summary>
        /// The board ID that contains this column
        /// </summary>
        [Required]
        public int BoardId { get; set; }

        /// <summary>
        /// Optional: New name for the board column
        /// </summary>
        [StringLength(100, MinimumLength = 1)]
        public string? BoardColumnName { get; set; }

        /// <summary>
        /// Optional: New color for the board column (hex format)
        /// </summary>
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be in hex format (e.g., #FF5733)")]
        public string? BoardColor { get; set; }

        /// <summary>
        /// Optional: New position for the board column
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Position must be greater than 0")]
        public int? Position { get; set; }

        /// <summary>
        /// Optional: New status name for the board column
        /// </summary>
        [StringLength(100, MinimumLength = 1)]
        public string? StatusName { get; set; }

        /// <summary>
        /// Optional: User ID who is updating the column
        /// </summary>
        public int? UpdatedBy { get; set; }
    }
}
