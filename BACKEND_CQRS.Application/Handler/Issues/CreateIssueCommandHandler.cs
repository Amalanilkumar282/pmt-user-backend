using AutoMapper;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Issues
{
    public class CreateIssueCommandHandler : IRequestHandler<CreateIssueCommand, ApiResponse<CreateIssueDto>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CreateIssueCommandHandler(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<CreateIssueDto>> Handle(CreateIssueCommand request, CancellationToken cancellationToken)
        {
            var issue = _mapper.Map<Issue>(request);
            issue.Id = Guid.NewGuid();
            issue.Type = request.IssueType;
            issue.StatusId = request.StatusId; // Set StatusId from request
            issue.CreatedAt = DateTimeOffset.UtcNow;

            _context.Issues.Add(issue);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<CreateIssueDto>(issue);
            return ApiResponse<CreateIssueDto>.Created(dto, "Issue created successfully");
        }
    }
}
