using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler
{
    public class GetUsersByProjectIdQueryHandler : IRequestHandler<GetUsersByProjectIdQuery, ApiResponse<List<UserDto>>>
    {
        private readonly AppDbContext _dbContext;

        public GetUsersByProjectIdQueryHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<List<UserDto>>> Handle(GetUsersByProjectIdQuery request, CancellationToken cancellationToken)
        {
            // Get distinct user ids from project_members for the project
            var userIds = await _dbContext.ProjectMembers
                .Where(pm => pm.ProjectId == request.ProjectId && pm.UserId.HasValue)
                .Select(pm => pm.UserId.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (userIds == null || !userIds.Any())
                return ApiResponse<List<UserDto>>.Fail("No users found for this project.");

            // Load users
            var users = await _dbContext.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync(cancellationToken);

            // Manual mapping to avoid adding AutoMapper profile
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                Name = u.Name,
                AvatarUrl = u.AvatarUrl,
                IsActive = u.IsActive ?? false,
                IsSuperAdmin = u.IsSuperAdmin ?? false,
                JiraId = u.JiraId,
                Type = u.Type,
                LastLogin = u.LastLogin
            }).ToList();

            return ApiResponse<List<UserDto>>.Success(userDtos);
        }
    }
}
