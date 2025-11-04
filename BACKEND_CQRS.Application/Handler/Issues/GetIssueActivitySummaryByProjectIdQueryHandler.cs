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
    public class GetIssueActivitySummaryByProjectQueryHandler : IRequestHandler<GetIssueActivitySummaryByProjectQuery, ApiResponse<Dictionary<string, int>>>
    {
        private readonly AppDbContext _context;

        public GetIssueActivitySummaryByProjectQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<Dictionary<string, int>>> Handle(GetIssueActivitySummaryByProjectQuery request, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var sevenDaysAgo = now.AddDays(-7);
            var sevenDaysAhead = now.AddDays(7);

            // filter by project
            var query = _context.Issues.AsNoTracking().Where(i => i.ProjectId == request.ProjectId);

            // 1️⃣ Created in last 7 days
            var createdCount = await query.CountAsync(i => i.CreatedAt != null && i.CreatedAt >= sevenDaysAgo, cancellationToken);

            // 2️⃣ Updated in last 7 days
            var updatedCount = await query.CountAsync(i => i.UpdatedAt != null && i.UpdatedAt >= sevenDaysAgo, cancellationToken);

            // 3️⃣ Completed (status = done) updated in last 7 days
            //   If "Done" is represented by specific StatusId, adjust the filter
            var completedCount = await query.CountAsync(
                i => i.Status != null && i.Status.StatusName.ToLower() == "done" && i.UpdatedAt >= sevenDaysAgo,
                cancellationToken
            );

            // 4️⃣ Due soon (within next 7 days)
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
