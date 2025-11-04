using AutoMapper;
using BACKEND_CQRS.Application.Dto;
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
    public class GetRecentIssuesByProjectIdQueryHandler : IRequestHandler<GetRecentIssuesQuery, ApiResponse<List<IssueDto>>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public GetRecentIssuesByProjectIdQueryHandler(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<IssueDto>>> Handle(GetRecentIssuesQuery request, CancellationToken cancellationToken)
        {
            var issues = await _context.Issues
                .AsNoTracking()
                .Include(i => i.Status)
                .Include(i => i.Assignee)
                .Include(i => i.Sprint)
                .Include(i => i.Epic)
                .Where(i => i.ProjectId == request.ProjectId)
                .OrderByDescending(i => i.UpdatedAt)
                .Take(request.Count)
                .ToListAsync(cancellationToken);

            if (issues == null || !issues.Any())
                return ApiResponse<List<IssueDto>>.Fail("No recent issues found for this project.");

            var issueDtos = _mapper.Map<List<IssueDto>>(issues);
            return ApiResponse<List<IssueDto>>.Success(issueDtos);
        }
    }
}
