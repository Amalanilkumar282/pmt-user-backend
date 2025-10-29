using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace BACKEND_CQRS.Application.Command
{
    /// <summary>
    /// Command to create a new board in a project
    /// </summary>
    public class CreateBoardCommand : IRequest<ApiResponse<CreateBoardResponseDto>>
    {
        /// <summary>
        /// The project ID to which the board belongs
        /// </summary>
        [Required(ErrorMessage = "Project ID is required")]
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Board name
        /// </summary>
        [Required(ErrorMessage = "Board name is required")]
        [StringLength(150, MinimumLength = 1, ErrorMessage = "Board name must be between 1 and 150 characters")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Board description (optional)
        /// </summary>
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        /// <summary>
        /// Board type: "team" or "custom"
        /// Default is "kanban" for backward compatibility, but will be set based on TeamId
        /// </summary>
        [Required(ErrorMessage = "Board type is required")]
        [RegularExpression("^(team|custom|kanban|scrum)$", 
            ErrorMessage = "Board type must be one of: team, custom, kanban, scrum")]
        public string Type { get; set; } = "custom";

        /// <summary>
        /// Team ID for team-based boards. Must be null for custom boards.
        /// </summary>
        public int? TeamId { get; set; }

        /// <summary>
        /// User ID who is creating the board (for audit purposes)
        /// </summary>
        public int? CreatedBy { get; set; }

        /// <summary>
        /// Additional metadata in JSON format (optional)
        /// </summary>
        public string? Metadata { get; set; }

        public CreateBoardCommand()
        {
        }

        public CreateBoardCommand(
            Guid projectId, 
            string name, 
            string type, 
            int? teamId = null, 
            string? description = null, 
            int? createdBy = null,
            string? metadata = null)
        {
            ProjectId = projectId;
            Name = name;
            Type = type;
            TeamId = teamId;
            Description = description;
            CreatedBy = createdBy;
            Metadata = metadata;
        }
    }
}
