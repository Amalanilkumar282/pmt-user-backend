using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Issues
{
    public class DeleteIssueCommandHandler : IRequestHandler<DeleteIssueCommand, ApiResponse<bool>>
    {
        private readonly AppDbContext _context;

        public DeleteIssueCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteIssueCommand request, CancellationToken cancellationToken)
        {
            var issue = await _context.Issues
                .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

            if (issue == null)
            {
                return ApiResponse<bool>.Fail($"Issue with ID {request.Id} not found.");
            }

            _context.Issues.Remove(issue);
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Issue deleted successfully");
        }
    }
}
