using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Application.Handler.ProjectMembers
{
    /// <summary>
    /// Handler for adding a member to a project
    /// </summary>
    public class AddProjectMemberCommandHandler : IRequestHandler<AddProjectMemberCommand, ApiResponse<AddProjectMemberResponseDto>>
    {
        private readonly IGenericRepository<Domain.Entities.ProjectMembers> _projectMemberRepository;
        private readonly IGenericRepository<Domain.Entities.Projects> _projectRepository;
        private readonly IGenericRepository<Domain.Entities.Users> _userRepository;
        private readonly IGenericRepository<Domain.Entities.Role> _roleRepository;
        private readonly ILogger<AddProjectMemberCommandHandler> _logger;

        public AddProjectMemberCommandHandler(
            IGenericRepository<Domain.Entities.ProjectMembers> projectMemberRepository,
            IGenericRepository<Domain.Entities.Projects> projectRepository,
            IGenericRepository<Domain.Entities.Users> userRepository,
            IGenericRepository<Domain.Entities.Role> roleRepository,
            ILogger<AddProjectMemberCommandHandler> logger)
        {
            _projectMemberRepository = projectMemberRepository ?? throw new ArgumentNullException(nameof(projectMemberRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<AddProjectMemberResponseDto>> Handle(
            AddProjectMemberCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Processing add project member request - Project: {ProjectId}, User: {UserId}, Role: {RoleId}, AddedBy: {AddedBy}",
                    request.ProjectId, request.UserId, request.RoleId, request.AddedBy);

                // Step 1: Validate that AddedBy is valid (>0) - SECURITY FIX
                if (request.AddedBy <= 0)
                {
                    _logger.LogWarning("Invalid AddedBy value: {AddedBy}", request.AddedBy);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        "AddedBy must be a valid user ID greater than 0. You must specify who is adding this member.");
                }

                // Step 2: Validate project exists and is NOT deleted - SECURITY FIX
                var projects = await _projectRepository.FindAsync(p => p.Id == request.ProjectId);
                var project = projects.FirstOrDefault();

                if (project == null)
                {
                    _logger.LogWarning("Project {ProjectId} does not exist", request.ProjectId);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"Project with ID {request.ProjectId} does not exist");
                }

                // ?? SECURITY: Check if project is soft-deleted
                if (project.DeletedAt.HasValue)
                {
                    _logger.LogWarning("SECURITY: Attempt to add member to deleted project {ProjectId} (deleted at {DeletedAt})", 
                        request.ProjectId, project.DeletedAt);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"Project {project.Name} has been deleted and cannot accept new members");
                }

                // Step 3: Validate AddedBy user exists, is active, and NOT deleted - SECURITY FIX
                var addedByUsers = await _userRepository.FindAsync(u => u.Id == request.AddedBy);
                var addedByUser = addedByUsers.FirstOrDefault();

                if (addedByUser == null)
                {
                    _logger.LogWarning("AddedBy user {UserId} does not exist", request.AddedBy);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"User with ID {request.AddedBy} does not exist");
                }

                // ?? SECURITY: Check if addedBy user is soft-deleted
                if (addedByUser.DeletedAt.HasValue)
                {
                    _logger.LogWarning("SECURITY: Deleted user {UserId} attempted to add member (deleted at {DeletedAt})", 
                        request.AddedBy, addedByUser.DeletedAt);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"User account {addedByUser.Name} has been deleted and cannot perform this action");
                }

                if (addedByUser.IsActive == false)
                {
                    _logger.LogWarning("AddedBy user {UserId} is not active", request.AddedBy);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"User {addedByUser.Name} is not active and cannot add members");
                }

                // Step 4: AUTHORIZATION - Check if AddedBy user is a project member
                var addedByMembership = await _projectMemberRepository.FindAsync(
                    pm => pm.ProjectId == request.ProjectId && pm.UserId == request.AddedBy);
                
                var addedByMember = addedByMembership.FirstOrDefault();

                if (addedByMember == null)
                {
                    _logger.LogWarning(
                        "AUTHORIZATION FAILED: User {AddedBy} ({AddedByName}) is not a member of project {ProjectId} ({ProjectName}) and cannot add members",
                        request.AddedBy, addedByUser.Name, request.ProjectId, project.Name);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"Unauthorized: {addedByUser.Name} is not a member of project {project.Name} and cannot add members");
                }

                // Step 5: CRITICAL AUTHORIZATION - Check if AddedBy user is a project owner OR super admin - ENHANCED
                bool isSuperAdmin = addedByUser.IsSuperAdmin == true;
                bool isProjectOwner = addedByMember.IsOwner == true;

                if (!isProjectOwner && !isSuperAdmin)
                {
                    _logger.LogWarning(
                        "AUTHORIZATION FAILED: User {AddedBy} ({AddedByName}) is a member but NOT an owner (is_owner={IsOwner}) or super admin (is_super_admin={IsSuperAdmin}) of project {ProjectId} ({ProjectName}) and cannot add members",
                        request.AddedBy, addedByUser.Name, addedByMember.IsOwner, isSuperAdmin, request.ProjectId, project.Name);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"Unauthorized: {addedByUser.Name} is not a project owner (is_owner=false). Only project owners can add members. Current owner status: is_owner={addedByMember.IsOwner}");
                }

                var authorizationType = isSuperAdmin ? "super admin" : "project owner";
                _logger.LogInformation(
                    "AUTHORIZATION SUCCESS: User {AddedBy} ({AddedByName}) is verified as a {AuthType} and can add members",
                    request.AddedBy, addedByUser.Name, authorizationType);

                // Step 6: Validate user to be added exists, is active, and NOT deleted - SECURITY FIX
                var users = await _userRepository.FindAsync(u => u.Id == request.UserId);
                var user = users.FirstOrDefault();

                if (user == null)
                {
                    _logger.LogWarning("User {UserId} does not exist", request.UserId);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"User with ID {request.UserId} does not exist");
                }

                // ?? SECURITY: Check if user to be added is soft-deleted
                if (user.DeletedAt.HasValue)
                {
                    _logger.LogWarning("SECURITY: Attempt to add deleted user {UserId} (deleted at {DeletedAt})", 
                        request.UserId, user.DeletedAt);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"User {user.Name} has been deleted and cannot be added to projects");
                }

                if (user.IsActive == false)
                {
                    _logger.LogWarning("User {UserId} is not active", request.UserId);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"User {user.Name} (ID: {request.UserId}) is not active and cannot be added to the project");
                }

                // Step 7: Validate role exists
                var roles = await _roleRepository.FindAsync(r => r.Id == request.RoleId);
                var role = roles.FirstOrDefault();

                if (role == null)
                {
                    _logger.LogWarning("Role {RoleId} does not exist", request.RoleId);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"Role with ID {request.RoleId} does not exist");
                }

                // Step 8: Check if user is already a member of the project
                var existingMembers = await _projectMemberRepository.FindAsync(
                    pm => pm.ProjectId == request.ProjectId && pm.UserId == request.UserId);

                if (existingMembers.Any())
                {
                    _logger.LogWarning(
                        "User {UserId} is already a member of project {ProjectId}",
                        request.UserId, request.ProjectId);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"User {user.Name} is already a member of project {project.Name}");
                }

                // Step 9: Determine if user being added is owner (project manager)
                bool isOwner = project.ProjectManagerId.HasValue && project.ProjectManagerId.Value == request.UserId;

                _logger.LogInformation(
                    "User {UserId} owner status: {IsOwner}, Project Manager ID: {ProjectManagerId}",
                    request.UserId, isOwner, project.ProjectManagerId);

                // Step 10: Create project member entity
                var projectMember = new Domain.Entities.ProjectMembers
                {
                    ProjectId = request.ProjectId,
                    UserId = request.UserId,
                    RoleId = request.RoleId,
                    IsOwner = isOwner,
                    AddedAt = DateTimeOffset.UtcNow,
                    AddedBy = request.AddedBy
                };

                _logger.LogInformation(
                    "Adding user {UserId} ({UserName}) to project {ProjectId} ({ProjectName}) with role {RoleId} ({RoleName}) by {AuthType} {AddedBy} ({AddedByName})",
                    user.Id, user.Name, project.Id, project.Name, role.Id, role.Name, authorizationType, request.AddedBy, addedByUser.Name);

                // Step 11: Save to database
                var createdMember = await _projectMemberRepository.CreateAsync(projectMember);

                _logger.LogInformation(
                    "Successfully added member {MemberId} - User {UserId} ({UserName}) to project {ProjectId} ({ProjectName}) by {AuthType} {AddedBy} ({AddedByName})",
                    createdMember.Id, createdMember.UserId, user.Name, createdMember.ProjectId, project.Name, authorizationType, request.AddedBy, addedByUser.Name);

                // Step 12: Prepare response
                var response = new AddProjectMemberResponseDto
                {
                    MemberId = createdMember.Id,
                    ProjectId = createdMember.ProjectId,
                    ProjectName = project.Name,
                    UserId = user.Id,
                    UserName = user.Name,
                    UserEmail = user.Email,
                    RoleId = role.Id,
                    RoleName = role.Name,
                    IsOwner = isOwner,
                    AddedAt = createdMember.AddedAt ?? DateTimeOffset.UtcNow,
                    AddedBy = createdMember.AddedBy,
                    AddedByName = addedByUser.Name
                };

                var ownerStatus = isOwner ? "Project Owner/Manager" : "Member";
                var message = $"{ownerStatus} {user.Name} successfully added to project {project.Name} with role {role.Name} by {authorizationType} {addedByUser.Name}";

                return ApiResponse<AddProjectMemberResponseDto>.Created(response, message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Database error while adding member to project {ProjectId}", request.ProjectId);
                return ApiResponse<AddProjectMemberResponseDto>.Fail(
                    "A database error occurred while adding the member. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding member to project {ProjectId}", request.ProjectId);
                return ApiResponse<AddProjectMemberResponseDto>.Fail(
                    "An unexpected error occurred while adding the member. Please contact support if the issue persists.");
            }
        }
    }
}
