using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Issues;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
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
        private readonly IIssueRepository _issueRepository;

        public GetIssuesByUserIdQueryHandler(IMapper mapper, IIssueRepository issueRepository)
        {
            _mapper = mapper;
            _issueRepository = issueRepository;
        }

        public async Task<ApiResponse<List<IssueDto>>> Handle(GetIssuesByUserIdQuery request, CancellationToken cancellationToken)
        {
            // Fetch all issues where user is the assignee
            var issues = await _issueRepository.FindAsync(i => i.AssigneeId == request.UserId);

            if (issues == null || !issues.Any())
            {
                return ApiResponse<List<IssueDto>>.Fail("No issues found for this user.");
            }

            var issueDtos = _mapper.Map<List<IssueDto>>(issues);
            return ApiResponse<List<IssueDto>>.Success(issueDtos);
        }
    }
}
