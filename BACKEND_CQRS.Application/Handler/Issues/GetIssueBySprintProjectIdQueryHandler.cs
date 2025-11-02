using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Issues;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Issues
{
    public class GetIssueBySprintProjectIdQueryHandler
      : IRequestHandler<GetIssueBySprintProjectIdQuery, ApiResponse<List<IssueDto>>>
    {
        private readonly IMapper _mapper;
        private readonly IIssueRepository _issueRepository;
        private readonly AppDbContext _dbContext; // 👈 Add DbContext reference

        public GetIssueBySprintProjectIdQueryHandler(IMapper mapper, IIssueRepository issueRepository, AppDbContext dbContext)
        {
            _mapper = mapper;
            _issueRepository = issueRepository;
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<List<IssueDto>>> Handle(GetIssueBySprintProjectIdQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Issue> query = _dbContext.Issues
                .Include(i => i.Status); // 👈 This ensures Status is loaded

            if (request.SprintId.HasValue)
            {
                query = query.Where(i => i.ProjectId == request.ProjectId && i.SprintId == request.SprintId);
            }
            else
            {
                query = query.Where(i => i.ProjectId == request.ProjectId);
            }

            var issues = await query.ToListAsync(cancellationToken);

            if (!issues.Any())
            {
                return ApiResponse<List<IssueDto>>.Fail("No issues found for the specified project/sprint.");
            }

            var issueDtos = _mapper.Map<List<IssueDto>>(issues);
            return ApiResponse<List<IssueDto>>.Success(issueDtos);
        }
    }
}
