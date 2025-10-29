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
    public class GetIssuesByProjectAndUserQueryHandler
        : IRequestHandler<GetIssuesByProjectAndUserQuery, ApiResponse<List<IssueDto>>>
    {
        private readonly IMapper _mapper;
        private readonly IIssueRepository _issueRepository;

        public GetIssuesByProjectAndUserQueryHandler(IMapper mapper, IIssueRepository issueRepository)
        {
            _mapper = mapper;
            _issueRepository = issueRepository;
        }

        public async Task<ApiResponse<List<IssueDto>>> Handle(GetIssuesByProjectAndUserQuery request, CancellationToken cancellationToken)
        {
            // Fetch issues matching both project and assignee (user)
            var issues = await _issueRepository.FindAsync(i => 
                i.ProjectId == request.ProjectId && 
                i.AssigneeId == request.UserId);

            if (issues == null || !issues.Any())
            {
                return ApiResponse<List<IssueDto>>.Fail("No issues found for the specified project and user.");
            }

            var issueDtos = _mapper.Map<List<IssueDto>>(issues);
            return ApiResponse<List<IssueDto>>.Success(issueDtos);
        }
    }
}
