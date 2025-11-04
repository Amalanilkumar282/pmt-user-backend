using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace BACKEND_CQRS.Application.Query.Permissions
{
    /// <summary>
    /// Query to get a user's permissions for a specific project
    /// </summary>
    public class GetUserProjectPermissionsQuery : IRequest<ApiResponse<UserProjectPermissionsDto>>
    {
        /// <summary>
        /// User ID to get permissions for
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be greater than 0")]
        public int UserId { get; set; }

        /// <summary>
        /// Project ID to get permissions for
        /// </summary>
        [Required]
        public Guid ProjectId { get; set; }

        public GetUserProjectPermissionsQuery(int userId, Guid projectId)
        {
            UserId = userId;
            ProjectId = projectId;
        }

        // Parameterless constructor for model binding
        public GetUserProjectPermissionsQuery()
        {
        }
    }
}
