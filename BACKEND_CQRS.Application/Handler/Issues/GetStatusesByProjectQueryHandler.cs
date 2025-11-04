using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Issues;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Issues
{
    public class GetStatusesByProjectQueryHandler 
        : IRequestHandler<GetStatusesByProjectQuery, ApiResponse<List<StatusDto>>>
    {
        private readonly AppDbContext _dbContext;

        public GetStatusesByProjectQueryHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<List<StatusDto>>> Handle(
            GetStatusesByProjectQuery request, 
            CancellationToken cancellationToken)
        {
            // Get all distinct status IDs used by issues in this project
            var statusIds = await _dbContext.Issues
                .Where(i => i.ProjectId == request.ProjectId && i.StatusId != null)
                .Select(i => i.StatusId.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (!statusIds.Any())
            {
                return ApiResponse<List<StatusDto>>.Success(
                    new List<StatusDto>(),
                    "No statuses found for the specified project.");
            }

            // Fetch the actual status details from the status table
            var statuses = await _dbContext.Statuses
                .Where(s => statusIds.Contains(s.Id))
                .Select(s => new StatusDto
                {
                    Id = s.Id,
                    StatusName = s.StatusName
                })
                .OrderBy(s => s.Id)
                .ToListAsync(cancellationToken);

            return ApiResponse<List<StatusDto>>.Success(
                statuses,
                $"Successfully retrieved {statuses.Count} unique status(es) for project.");
        }
    }
}