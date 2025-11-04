using BACKEND_CQRS.Application.Query.Issues;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Issues
{
    public class GetIssueActivitySummaryBySprintIdQueryHandler : IRequestHandler<GetIssueActivitySummaryBysprintIdQuery, ApiResponse<Dictionary<string, int>>>
    {
        private readonly AppDbContext _context;

        public GetIssueActivitySummaryBySprintIdQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<Dictionary<string, int>>> Handle(GetIssueActivitySummaryBysprintIdQuery request, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var sevenDaysAgo = now.AddDays(-7);
            var sevenDaysAhead = now.AddDays(7);

            var query = _context.Issues
                .AsNoTracking()
                .Include(i => i.Status)
                .Where(i => i.ProjectId == request.ProjectId);

            // 🔹 Filter by Sprint if provided
            if (request.SprintId.HasValue)
                query = query.Where(i => i.SprintId == request.SprintId.Value);

            // 🟩 Created in last 7 days
            var createdCount = await query.CountAsync(i => i.CreatedAt != null && i.CreatedAt >= sevenDaysAgo, cancellationToken);

            // 🟨 Updated in last 7 days
            var updatedCount = await query.CountAsync(i => i.UpdatedAt != null && i.UpdatedAt >= sevenDaysAgo, cancellationToken);

            // 🟦 Completed (status = "Done") updated in last 7 days
            var completedCount = await query.CountAsync(
                i => i.Status != null && i.Status.StatusName.ToLower() == "done" && i.UpdatedAt >= sevenDaysAgo,
                cancellationToken
            );

            // 🟥 Due soon (within next 7 days)
            var dueSoonCount = await query.CountAsync(
                i => i.DueDate != null && i.DueDate >= now && i.DueDate <= sevenDaysAhead,
                cancellationToken
            );

            var data = new Dictionary<string, int>
            {
                { "created", createdCount },
                { "updated", updatedCount },
                { "completed", completedCount },
                { "dueSoon", dueSoonCount }
            };

            return ApiResponse<Dictionary<string, int>>.Success(data);
        }
    }
}
