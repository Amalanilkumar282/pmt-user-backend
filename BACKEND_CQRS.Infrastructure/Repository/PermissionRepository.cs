using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Infrastructure.Repository
{
    /// <summary>
    /// Repository implementation for Permission-related operations
    /// </summary>
    public class PermissionRepository : IPermissionRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PermissionRepository>? _logger;

        public PermissionRepository(AppDbContext context, ILogger<PermissionRepository>? logger = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        /// <summary>
        /// Get all permissions for a user in a specific project based on their role
        /// </summary>
        public async Task<List<Permission>> GetUserPermissionsForProjectAsync(int userId, Guid projectId)
        {
            try
            {
                _logger?.LogInformation(
                    "Fetching permissions for UserId: {UserId} in ProjectId: {ProjectId}",
                    userId, projectId);

                // Query to get permissions through the user's role in the project
                var permissions = await _context.ProjectMembers
                    .AsNoTracking()
                    .Where(pm => pm.UserId == userId && pm.ProjectId == projectId && pm.RoleId != null)
                    .SelectMany(pm => _context.Set<RolePermission>()
                        .Where(rp => rp.RoleId == pm.RoleId)
                        .Select(rp => rp.Permission))
                    .Distinct()
                    .ToListAsync();

                _logger?.LogInformation(
                    "Found {Count} permission(s) for UserId: {UserId} in ProjectId: {ProjectId}",
                    permissions.Count, userId, projectId);

                return permissions;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, 
                    "Error fetching permissions for UserId: {UserId} in ProjectId: {ProjectId}",
                    userId, projectId);
                throw new InvalidOperationException(
                    "An error occurred while fetching user permissions. See inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Get user's role information in a project
        /// </summary>
        public async Task<(int? roleId, string? roleName, bool isOwner, DateTimeOffset? addedAt)> GetUserRoleInProjectAsync(int userId, Guid projectId)
        {
            try
            {
                _logger?.LogInformation(
                    "Fetching role for UserId: {UserId} in ProjectId: {ProjectId}",
                    userId, projectId);

                var memberInfo = await _context.ProjectMembers
                    .AsNoTracking()
                    .Where(pm => pm.UserId == userId && pm.ProjectId == projectId)
                    .Select(pm => new
                    {
                        pm.RoleId,
                        RoleName = pm.Role != null ? pm.Role.Name : null,
                        IsOwner = pm.IsOwner ?? false,
                        pm.AddedAt
                    })
                    .FirstOrDefaultAsync();

                if (memberInfo == null)
                {
                    _logger?.LogWarning(
                        "User {UserId} is not a member of project {ProjectId}",
                        userId, projectId);
                    return (null, null, false, null);
                }

                return (memberInfo.RoleId, memberInfo.RoleName, memberInfo.IsOwner, memberInfo.AddedAt);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "Error fetching role for UserId: {UserId} in ProjectId: {ProjectId}",
                    userId, projectId);
                throw new InvalidOperationException(
                    "An error occurred while fetching user role. See inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Check if a user is a member of a project
        /// </summary>
        public async Task<bool> IsUserProjectMemberAsync(int userId, Guid projectId)
        {
            try
            {
                return await _context.ProjectMembers
                    .AsNoTracking()
                    .AnyAsync(pm => pm.UserId == userId && pm.ProjectId == projectId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex,
                    "Error checking project membership for UserId: {UserId} in ProjectId: {ProjectId}",
                    userId, projectId);
                throw new InvalidOperationException(
                    "An error occurred while checking project membership. See inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Get all permissions
        /// </summary>
        public async Task<List<Permission>> GetAllPermissionsAsync()
        {
            try
            {
                return await _context.Set<Permission>()
                    .AsNoTracking()
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching all permissions");
                throw new InvalidOperationException(
                    "An error occurred while fetching permissions. See inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Get permissions by role ID
        /// </summary>
        public async Task<List<Permission>> GetPermissionsByRoleIdAsync(int roleId)
        {
            try
            {
                _logger?.LogInformation("Fetching permissions for RoleId: {RoleId}", roleId);

                var permissions = await _context.Set<RolePermission>()
                    .AsNoTracking()
                    .Where(rp => rp.RoleId == roleId)
                    .Select(rp => rp.Permission)
                    .Distinct()
                    .ToListAsync();

                _logger?.LogInformation(
                    "Found {Count} permission(s) for RoleId: {RoleId}",
                    permissions.Count, roleId);

                return permissions;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching permissions for RoleId: {RoleId}", roleId);
                throw new InvalidOperationException(
                    "An error occurred while fetching role permissions. See inner exception for details.", ex);
            }
        }
    }
}
