using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace BACKEND_CQRS.Application.Command
{
    /// <summary>
    /// Command to add a member to a project
    /// </summary>
    public class AddProjectMemberCommand : IRequest<ApiResponse<AddProjectMemberResponseDto>>
    {
        /// <summary>
        /// The project ID to add the member to
        /// </summary>
        [Required(ErrorMessage = "Project ID is required")]
        public Guid ProjectId { get; set; }

        /// <summary>
        /// The user ID of the member to add
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be greater than 0")]
        public int UserId { get; set; }

        /// <summary>
        /// The role ID for the member in this project
        /// </summary>
        [Required(ErrorMessage = "Role ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Role ID must be greater than 0")]
        public int RoleId { get; set; }

        /// <summary>
        /// User ID who is adding this member (REQUIRED - must be a project owner)
        /// </summary>
        [Required(ErrorMessage = "AddedBy is required. Only project owners can add members.")]
        [Range(1, int.MaxValue, ErrorMessage = "AddedBy must be greater than 0")]
        public int AddedBy { get; set; }

        public AddProjectMemberCommand()
        {
        }

        public AddProjectMemberCommand(Guid projectId, int userId, int roleId, int addedBy)
        {
            ProjectId = projectId;
            UserId = userId;
            RoleId = roleId;
            AddedBy = addedBy;
        }
    }
}
