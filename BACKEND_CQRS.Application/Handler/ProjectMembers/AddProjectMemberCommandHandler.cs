using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace BACKEND_CQRS.Application.Handler.ProjectMembers
{
    /// <summary>
    /// Handler for adding a member to a project - SIMPLIFIED VERSION
    /// </summary>
    public class AddProjectMemberCommandHandler : IRequestHandler<AddProjectMemberCommand, ApiResponse<AddProjectMemberResponseDto>>
    {
        private readonly IGenericRepository<Domain.Entities.ProjectMembers> _projectMemberRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<AddProjectMemberCommandHandler> _logger;

        public AddProjectMemberCommandHandler(
            IGenericRepository<Domain.Entities.ProjectMembers> projectMemberRepository,
            AppDbContext context,
            ILogger<AddProjectMemberCommandHandler> logger)
        {
            _projectMemberRepository = projectMemberRepository ?? throw new ArgumentNullException(nameof(projectMemberRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<AddProjectMemberResponseDto>> Handle(
            AddProjectMemberCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Processing add project member - Project: {ProjectId}, User: {UserId}, Role: {RoleId}, AddedBy: {AddedBy}",
                    request.ProjectId, request.UserId, request.RoleId, request.AddedBy);

                // Step 1: Validate project exists
                var project = await _context.Projects
                    .Where(p => p.Id == request.ProjectId)
                    .Select(p => new { p.Id, p.Name, p.ProjectManagerId })
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);

                if (project == null)
                {
                    _logger.LogWarning("Project {ProjectId} does not exist", request.ProjectId);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"Project with ID {request.ProjectId} does not exist");
                }

                // Step 2: Validate user to be added exists and is active
                var user = await _context.Users
                    .Where(u => u.Id == request.UserId)
                    .Select(u => new { u.Id, u.Name, u.Email, u.IsActive })
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning("User {UserId} does not exist", request.UserId);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"User with ID {request.UserId} does not exist");
                }

                if (user.IsActive == false)
                {
                    _logger.LogWarning("User {UserId} is not active", request.UserId);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"User {user.Name} is not active and cannot be added to the project");
                }

                // Step 3: Validate AddedBy user exists (the one adding the member)
                var addedByUser = await _context.Users
                    .Where(u => u.Id == request.AddedBy)
                    .Select(u => new { u.Id, u.Name })
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);

                if (addedByUser == null)
                {
                    _logger.LogWarning("AddedBy user {UserId} does not exist", request.AddedBy);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"User with ID {request.AddedBy} does not exist");
                }

                // Step 4: Validate role exists
                var role = await _context.Roles
                    .Where(r => r.Id == request.RoleId)
                    .Select(r => new { r.Id, r.Name })
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);

                if (role == null)
                {
                    _logger.LogWarning("Role {RoleId} does not exist", request.RoleId);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"Role with ID {request.RoleId} does not exist");
                }

                // Step 5: Check if user is already a member of the project
                var existingMember = await _context.ProjectMembers
                    .Where(pm => pm.ProjectId == request.ProjectId && pm.UserId == request.UserId)
                    .AsNoTracking()
                    .AnyAsync(cancellationToken);

                if (existingMember)
                {
                    _logger.LogWarning("User {UserId} is already a member of project {ProjectId}", 
                        request.UserId, request.ProjectId);
                    return ApiResponse<AddProjectMemberResponseDto>.Fail(
                        $"User {user.Name} is already a member of project {project.Name}");
                }

                // Step 6: Determine if user being added is the project manager (owner)
                bool isOwner = project.ProjectManagerId.HasValue && 
                               project.ProjectManagerId.Value == request.UserId;

                _logger.LogInformation(
                    "Adding user {UserId} ({UserName}) to project {ProjectId} ({ProjectName}) with role {RoleId} ({RoleName}). IsOwner: {IsOwner}",
                    user.Id, user.Name, project.Id, project.Name, role.Id, role.Name, isOwner);

                // Step 7: Create project member entity
                // TeamId will be NULL by default - we don't set it
                var projectMember = new Domain.Entities.ProjectMembers
                {
                    ProjectId = request.ProjectId,
                    UserId = request.UserId,
                    RoleId = request.RoleId,
                    IsOwner = isOwner,
                    AddedAt = DateTimeOffset.UtcNow,
                    AddedBy = request.AddedBy
                    // TeamId is intentionally not set - will be NULL
                };

                // Step 8: Save to database
                var createdMember = await _projectMemberRepository.CreateAsync(projectMember);

                _logger.LogInformation(
                    "Successfully added member {MemberId} - User {UserId} ({UserName}) to project {ProjectId} ({ProjectName})",
                    createdMember.Id, createdMember.UserId, user.Name, createdMember.ProjectId, project.Name);

                // Step 9: Prepare response
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

                var ownerStatus = isOwner ? "as Project Owner/Manager" : "as Member";
                var message = $"{user.Name} successfully added to project {project.Name} {ownerStatus} with role {role.Name}";

                return ApiResponse<AddProjectMemberResponseDto>.Created(response, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while adding member to project {ProjectId}", request.ProjectId);
                return ApiResponse<AddProjectMemberResponseDto>.Fail(
                    "An error occurred while adding the member. Please contact support if the issue persists.");
            }
        }
    }
}
