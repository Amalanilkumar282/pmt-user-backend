        using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler
{
    public class GetUsersByProjectIdQueryHandler : IRequestHandler<GetUsersByProjectIdQuery, ApiResponse<List<ProjectUserDto>>>
    {
        private readonly AppDbContext _dbContext;

        public GetUsersByProjectIdQueryHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<List<ProjectUserDto>>> Handle(GetUsersByProjectIdQuery request, CancellationToken cancellationToken)
        {
            // Get project members with user, role, and addedBy user information
            var projectMembers = await _dbContext.ProjectMembers
                .Include(pm => pm.User)
                .Include(pm => pm.Role)
                .Include(pm => pm.Project)
                .Where(pm => pm.ProjectId == request.ProjectId && pm.UserId.HasValue)
                .ToListAsync(cancellationToken);

            if (projectMembers == null || !projectMembers.Any())
                return ApiResponse<List<ProjectUserDto>>.Fail("No users found for this project.");

            // Get all user IDs to fetch AddedBy user names
            var addedByUserIds = projectMembers
                .Where(pm => pm.AddedBy.HasValue)
                .Select(pm => pm.AddedBy.Value)
                .Distinct()
                .ToList();

            var addedByUsers = await _dbContext.Users
                .Where(u => addedByUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Name, cancellationToken);

            // Get teams for all users in this project
            var userIds = projectMembers.Select(pm => pm.UserId.Value).Distinct().ToList();
            
            var userTeams = await _dbContext.TeamMembers
                .Include(tm => tm.Team)
                .Include(tm => tm.ProjectMember)
                .Where(tm => tm.Team.ProjectId == request.ProjectId 
                          && tm.ProjectMember.UserId.HasValue 
                          && userIds.Contains(tm.ProjectMember.UserId.Value))
                .Select(tm => new
                {
                    UserId = tm.ProjectMember.UserId.Value,
                    TeamId = tm.TeamId,
                    TeamName = tm.Team.Name
                })
                .ToListAsync(cancellationToken);

            // Group teams by user
            var userTeamsGrouped = userTeams
                .GroupBy(ut => ut.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(ut => new UserTeamDto
                    {
                        TeamId = ut.TeamId,
                        TeamName = ut.TeamName
                    }).ToList()
                );

            // Map to DTOs
            var projectUserDtos = projectMembers.Select(pm => new ProjectUserDto
            {
                ProjectMemberId = pm.Id,
                Id = pm.User.Id,
                Email = pm.User.Email,
                Name = pm.User.Name,
                AvatarUrl = pm.User.AvatarUrl,
                IsActive = pm.User.IsActive ?? false,
                IsSuperAdmin = pm.User.IsSuperAdmin ?? false,
                JiraId = pm.User.JiraId,
                Type = pm.User.Type,
                LastLogin = pm.User.LastLogin,
                
                // Project-specific fields
                RoleId = pm.RoleId,
                RoleName = pm.Role?.Name,
                IsOwner = pm.IsOwner,
                AddedAt = pm.AddedAt,
                AddedBy = pm.AddedBy,
                AddedByName = pm.AddedBy.HasValue && addedByUsers.ContainsKey(pm.AddedBy.Value) 
                    ? addedByUsers[pm.AddedBy.Value] 
                    : null,
                
                // Teams
                Teams = userTeamsGrouped.ContainsKey(pm.User.Id) 
                    ? userTeamsGrouped[pm.User.Id] 
                    : new List<UserTeamDto>()
            }).ToList();

            return ApiResponse<List<ProjectUserDto>>.Success(projectUserDtos);
        }
    }
}
