using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Issues;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
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

        public GetIssueBySprintProjectIdQueryHandler(IMapper mapper, IIssueRepository issueRepository)
        {
            _mapper = mapper;
            _issueRepository = issueRepository;
        }

        public async Task<ApiResponse<List<IssueDto>>> Handle(GetIssueBySprintProjectIdQuery request, CancellationToken cancellationToken)
        {
            List<Issue> issues;

            if (request.SprintId.HasValue)
            {
                // Fetch issues matching both project and sprint
                issues = await _issueRepository.FindAsync(i => i.ProjectId == request.ProjectId && i.SprintId == request.SprintId);
            }
            else
            {
                // Fetch all issues under the project (no sprint filter)
                issues = await _issueRepository.FindAsync(i => i.ProjectId == request.ProjectId);
            }

            if (issues == null || !issues.Any())
            {
                return ApiResponse<List<IssueDto>>.Fail("No issues found for the specified project/sprint.");
            }

            var issueDtos = _mapper.Map<List<IssueDto>>(issues);
            return ApiResponse<List<IssueDto>>.Success(issueDtos);
        }
    }
}
