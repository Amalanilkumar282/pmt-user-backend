using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.User;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.User
{
    public class GetUserActivitiesQueryHandler
        : IRequestHandler<GetUserActivitiesQuery, ApiResponse<List<ActivityLogDto>>>
    {
        private readonly AppDbContext _dbContext;

        public GetUserActivitiesQueryHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<List<ActivityLogDto>>> Handle(GetUserActivitiesQuery request, CancellationToken cancellationToken)
        {
            var activities = await _dbContext.ActivityLogs
                .Include(a => a.User)
                .Where(a => a.UserId == request.UserId)
                .OrderByDescending(a => a.CreatedAt)
                .Take(request.Take)
                .Select(a => new ActivityLogDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    UserName = a.User != null ? a.User.Name : null,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    Action = a.Action,
                    Description = a.Description,
                    CreatedAt = a.CreatedAt
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (activities == null || !activities.Any())
            {
                return ApiResponse<List<ActivityLogDto>>.Fail("No activities found for this user.");
            }

            return ApiResponse<List<ActivityLogDto>>.Success(activities);
        }
    }
}
