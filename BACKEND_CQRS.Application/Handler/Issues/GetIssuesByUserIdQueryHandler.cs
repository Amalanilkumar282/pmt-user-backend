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
    public class GetIssuesByUserIdQueryHandler
        : IRequestHandler<GetIssuesByUserIdQuery, ApiResponse<List<IssueDto>>>
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;

        public GetIssuesByUserIdQueryHandler(IMapper mapper, AppDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<List<IssueDto>>> Handle(GetIssuesByUserIdQuery request, CancellationToken cancellationToken)
        {
            // Fetch all issues where user is the assignee, including all necessary navigation properties
            var issues = await _dbContext.Issues
                .AsNoTracking()
                .Include(i => i.Status)
                .Include(i => i.Assignee)
                .Include(i => i.Sprint)
                .Include(i => i.Epic)
                .Where(i => i.AssigneeId == request.UserId)
                .ToListAsync(cancellationToken);

            if (issues == null || !issues.Any())
            {
                return ApiResponse<List<IssueDto>>.Fail("No issues found for this user.");
            }

            var issueDtos = _mapper.Map<List<IssueDto>>(issues);
            return ApiResponse<List<IssueDto>>.Success(issueDtos);
        }
    }
}
