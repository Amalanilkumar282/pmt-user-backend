using BACKEND_CQRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Persistance
{
    /// <summary>
    /// Repository interface for Permission-related operations
    /// </summary>
    public interface IPermissionRepository
    {
        /// <summary>
        /// Get all permissions for a user in a specific project based on their role
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="projectId">Project ID</param>
        /// <returns>List of permissions</returns>
        Task<List<Permission>> GetUserPermissionsForProjectAsync(int userId, Guid projectId);

        /// <summary>
        /// Get user's role information in a project
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="projectId">Project ID</param>
        /// <returns>Tuple containing role ID, role name, and isOwner flag</returns>
        Task<(int? roleId, string? roleName, bool isOwner, DateTimeOffset? addedAt)> GetUserRoleInProjectAsync(int userId, Guid projectId);

        /// <summary>
        /// Check if a user is a member of a project
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="projectId">Project ID</param>
        /// <returns>True if user is a member, false otherwise</returns>
        Task<bool> IsUserProjectMemberAsync(int userId, Guid projectId);

        /// <summary>
        /// Get all permissions
        /// </summary>
        /// <returns>List of all permissions</returns>
        Task<List<Permission>> GetAllPermissionsAsync();

        /// <summary>
        /// Get permissions by role ID
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>List of permissions for the role</returns>
        Task<List<Permission>> GetPermissionsByRoleIdAsync(int roleId);
    }
}
