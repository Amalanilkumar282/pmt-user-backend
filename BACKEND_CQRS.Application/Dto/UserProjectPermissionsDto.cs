using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Dto
{
    /// <summary>
    /// DTO containing user's role and permissions for a specific project
    /// </summary>
    public class UserProjectPermissionsDto
    {
        /// <summary>
        /// User ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// User's name
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// User's email
        /// </summary>
        public string UserEmail { get; set; } = string.Empty;

        /// <summary>
        /// Project ID
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Project name
        /// </summary>
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>
        /// Role ID assigned to user in this project
        /// </summary>
        public int? RoleId { get; set; }

        /// <summary>
        /// Role name assigned to user in this project
        /// </summary>
        public string? RoleName { get; set; }

        /// <summary>
        /// Whether user is the project owner
        /// </summary>
        public bool IsOwner { get; set; }

        /// <summary>
        /// When the user was added to the project
        /// </summary>
        public DateTimeOffset? AddedAt { get; set; }

        /// <summary>
        /// List of permission names granted to this user for this project
        /// </summary>
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();

        /// <summary>
        /// Quick lookup flags for common permissions (for frontend convenience)
        /// </summary>
        public PermissionFlags PermissionFlags { get; set; } = new PermissionFlags();
    }

    /// <summary>
    /// Individual permission details
    /// </summary>
    public class PermissionDto
    {
        /// <summary>
        /// Permission ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Permission name (e.g., "project.create", "user.manage")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Permission description
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// Boolean flags for quick permission checks in frontend
    /// </summary>
    public class PermissionFlags
    {
        public bool CanCreateProject { get; set; }
        public bool CanReadProject { get; set; }
        public bool CanUpdateProject { get; set; }
        public bool CanDeleteProject { get; set; }
        public bool CanManageTeams { get; set; }
        public bool CanManageUsers { get; set; }
    }
}
