using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace BACKEND_CQRS.Application.Command
{
    /// <summary>
    /// Command to update a board's properties
    /// </summary>
    public class UpdateBoardCommand : IRequest<ApiResponse<UpdateBoardResponseDto>>
    {
        /// <summary>
        /// The ID of the board to update (set from route parameter)
        /// </summary>
        public int BoardId { get; set; }

        /// <summary>
        /// Optional: New name for the board
        /// </summary>
        [StringLength(150, MinimumLength = 1, ErrorMessage = "Board name must be between 1 and 150 characters")]
        public string? Name { get; set; }

        /// <summary>
        /// Optional: New description for the board
        /// </summary>
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        /// <summary>
        /// Optional: New type for the board (kanban, scrum, team, custom)
        /// </summary>
        [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        [RegularExpression(@"^(kanban|scrum|team|custom)$", ErrorMessage = "Type must be one of: kanban, scrum, team, custom")]
        public string? Type { get; set; }

        /// <summary>
        /// Optional: New team ID for the board (null to remove team association)
        /// </summary>
        public int? TeamId { get; set; }

        /// <summary>
        /// Optional: Indicates if the board should be marked as active or inactive
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Optional: Metadata in JSON format
        /// </summary>
        public string? Metadata { get; set; }

        /// <summary>
        /// Optional: User ID who is updating the board
        /// </summary>
        public int? UpdatedBy { get; set; }

        /// <summary>
        /// Special flag to explicitly remove team association (set TeamId to null)
        /// </summary>
        public bool? RemoveTeamAssociation { get; set; }
    }
}
