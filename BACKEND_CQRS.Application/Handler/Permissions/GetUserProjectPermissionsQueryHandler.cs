using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Permissions;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Permissions
{
    /// <summary>
    /// Handler for GetUserProjectPermissionsQuery
    /// </summary>
    public class GetUserProjectPermissionsQueryHandler 
        : IRequestHandler<GetUserProjectPermissionsQuery, ApiResponse<UserProjectPermissionsDto>>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<GetUserProjectPermissionsQueryHandler> _logger;

        public GetUserProjectPermissionsQueryHandler(
            IPermissionRepository permissionRepository,
            IUserRepository userRepository,
            AppDbContext context,
            ILogger<GetUserProjectPermissionsQueryHandler> logger)
        {
            _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<UserProjectPermissionsDto>> Handle(
            GetUserProjectPermissionsQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Processing GetUserProjectPermissionsQuery for UserId: {UserId}, ProjectId: {ProjectId}",
                    request.UserId, request.ProjectId);

                // Validate inputs
                if (request.UserId <= 0)
                {
                    return ApiResponse<UserProjectPermissionsDto>.Fail(
                        "Invalid user ID. User ID must be greater than 0.");
                }

                if (request.ProjectId == Guid.Empty)
                {
                    return ApiResponse<UserProjectPermissionsDto>.Fail(
                        "Invalid project ID. Project ID cannot be empty.");
                }

                // Check if user exists
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    return ApiResponse<UserProjectPermissionsDto>.Fail(
                        $"User with ID {request.UserId} not found.");
                }

                // Check if project exists
                var project = await _context.Projects
                    .AsNoTracking()
                    .Where(p => p.Id == request.ProjectId)
                    .Select(p => new { p.Id, p.Name })
                    .FirstOrDefaultAsync(cancellationToken);

                if (project == null)
                {
                    return ApiResponse<UserProjectPermissionsDto>.Fail(
                        $"Project with ID {request.ProjectId} not found.");
                }

                // Check if user is a member of the project
                var isMember = await _permissionRepository.IsUserProjectMemberAsync(
                    request.UserId, request.ProjectId);

                if (!isMember)
                {
                    return ApiResponse<UserProjectPermissionsDto>.Fail(
                        $"User {user.Name} (ID: {request.UserId}) is not a member of project '{project.Name}'.");
                }

                // Get user's role in the project
                var (roleId, roleName, isOwner, addedAt) = await _permissionRepository
                    .GetUserRoleInProjectAsync(request.UserId, request.ProjectId);

                // Get user's permissions for the project
                var permissions = await _permissionRepository
                    .GetUserPermissionsForProjectAsync(request.UserId, request.ProjectId);

                // Map permissions to DTOs
                var permissionDtos = permissions.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description
                }).ToList();

                // Create permission flags for quick frontend checks
                var permissionFlags = new PermissionFlags
                {
                    CanCreateProject = permissions.Any(p => 
                        p.Name.Equals("project.create", StringComparison.OrdinalIgnoreCase)),
                    CanReadProject = permissions.Any(p => 
                        p.Name.Equals("project.read", StringComparison.OrdinalIgnoreCase)),
                    CanUpdateProject = permissions.Any(p => 
                        p.Name.Equals("project.update", StringComparison.OrdinalIgnoreCase)),
                    CanDeleteProject = permissions.Any(p => 
                        p.Name.Equals("project.delete", StringComparison.OrdinalIgnoreCase)),
                    CanManageTeams = permissions.Any(p => 
                        p.Name.Equals("team.manage", StringComparison.OrdinalIgnoreCase)),
                    CanManageUsers = permissions.Any(p => 
                        p.Name.Equals("user.manage", StringComparison.OrdinalIgnoreCase))
                };

                // Build response DTO
                var responseDto = new UserProjectPermissionsDto
                {
                    UserId = request.UserId,
                    UserName = user.Name ?? string.Empty,
                    UserEmail = user.Email ?? string.Empty,
                    ProjectId = request.ProjectId,
                    ProjectName = project.Name ?? string.Empty,
                    RoleId = roleId,
                    RoleName = roleName,
                    IsOwner = isOwner,
                    AddedAt = addedAt,
                    Permissions = permissionDtos,
                    PermissionFlags = permissionFlags
                };

                _logger.LogInformation(
                    "Successfully retrieved {PermissionCount} permission(s) for UserId: {UserId} in ProjectId: {ProjectId}. Role: {RoleName}, IsOwner: {IsOwner}",
                    permissionDtos.Count, request.UserId, request.ProjectId, roleName ?? "None", isOwner);

                return ApiResponse<UserProjectPermissionsDto>.Success(
                    responseDto,
                    $"Successfully retrieved permissions for user '{user.Name}' in project '{project.Name}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing GetUserProjectPermissionsQuery for UserId: {UserId}, ProjectId: {ProjectId}",
                    request.UserId, request.ProjectId);

                return ApiResponse<UserProjectPermissionsDto>.Fail(
                    "An unexpected error occurred while retrieving user permissions. Please try again later.");
            }
        }
    }
}
