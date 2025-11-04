using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.IssueComments
{
    public class DeleteIssueCommentCommandHandler : IRequestHandler<DeleteIssueCommentCommand, ApiResponse<Guid>>
    {
        private readonly AppDbContext _context;

        public DeleteIssueCommentCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<Guid>> Handle(DeleteIssueCommentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var comment = await _context.IssueComments
                    .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

                if (comment == null)
                {
                    return ApiResponse<Guid>.Fail("Comment not found");
                }

                // Delete associated mentions first
                var mentions = await _context.Mentions
                    .Where(m => m.IssueCommentsId == request.Id)
                    .ToListAsync(cancellationToken);
                _context.Mentions.RemoveRange(mentions);

                // Delete the comment
                _context.IssueComments.Remove(comment);
                await _context.SaveChangesAsync(cancellationToken);

                return ApiResponse<Guid>.Success(request.Id, "Comment deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<Guid>.Fail($"Error deleting comment: {ex.Message}");
            }
        }
    }
}
