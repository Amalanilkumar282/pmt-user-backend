using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Infrastructure.Repository
{
    public class ProjectMemberRepository : GenericRepository<ProjectMembers>
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProjectMemberRepository>? _logger;

        public ProjectMemberRepository(AppDbContext context, ILogger<ProjectMemberRepository>? logger = null) 
            : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        /// <summary>
        /// Safely find a project member by project ID and user ID without loading problematic navigation properties
        /// </summary>
        public async Task<ProjectMembers?> FindMemberByProjectAndUserAsync(Guid projectId, int userId)
        {
            try
            {
                _logger?.LogInformation("Finding project member - ProjectId: {ProjectId}, UserId: {UserId}", 
                    projectId, userId);

                // Use Select to only get the data we need, avoiding Teams.ProjectMembers navigation loading
                var memberData = await _context.ProjectMembers
                    .Where(pm => pm.ProjectId == projectId && pm.UserId == userId)
                    .Select(pm => new
                    {
                        Member = pm,
                        ProjectId = pm.ProjectId,
                        UserId = pm.UserId,
                        RoleId = pm.RoleId,
                        IsOwner = pm.IsOwner,
                        AddedBy = pm.AddedBy,
                        AddedAt = pm.AddedAt
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (memberData == null)
                {
                    _logger?.LogInformation("Project member not found");
                    return null;
                }

                _logger?.LogInformation("Found project member {MemberId}", memberData.Member.Id);
                return memberData.Member;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error finding project member");
                throw new InvalidOperationException(
                    "An error occurred while finding project member. See inner exception for details.", ex);
            }
        }

        /// <summary>
        /// Check if a user exists as a project member without loading navigation properties
        /// </summary>
        public async Task<bool> IsMemberOfProjectAsync(Guid projectId, int userId)
        {
            try
            {
                return await _context.ProjectMembers
                    .AsNoTracking()
                    .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking project membership");
                throw new InvalidOperationException(
                    "An error occurred while checking project membership. See inner exception for details.", ex);
            }
        }
    }
}
