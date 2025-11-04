using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BACKEND_CQRS.Domain.Entities;

namespace BACKEND_CQRS.Application.Handler.IssueComments
{
    public class UpdateIssueCommentCommandHandler : IRequestHandler<UpdateIssueCommentCommand, ApiResponse<Guid>>
    {
        private readonly AppDbContext _context;

        public UpdateIssueCommentCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<Guid>> Handle(UpdateIssueCommentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var comment = await _context.IssueComments
                    .Include(c => c.Mentions)
                    .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

                if (comment == null)
                {
                    return ApiResponse<Guid>.Fail("Comment not found");
                }

                // Update comment
                comment.Body = request.Body;
                comment.UpdatedBy = request.UpdatedBy;
                comment.UpdatedAt = DateTimeOffset.UtcNow;

                // Remove old mentions
                var existingMentions = await _context.Mentions
                    .Where(m => m.IssueCommentsId == comment.Id)
                    .ToListAsync(cancellationToken);
                _context.Mentions.RemoveRange(existingMentions);

                // Add new mentions
                if (request.MentionedUserIds != null && request.MentionedUserIds.Any())
                {
                    foreach (var userId in request.MentionedUserIds.Distinct())
                    {
                        var userExists = await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
                        if (userExists)
                        {
                            var mention = new Mention
                            {
                                Id = Guid.NewGuid(),
                                MentionUserId = userId,
                                IssueCommentsId = comment.Id,
                                CreatedBy = request.UpdatedBy,
                                UpdatedBy = request.UpdatedBy,
                                CreatedAt = DateTimeOffset.UtcNow,
                                UpdatedAt = DateTimeOffset.UtcNow
                            };
                            _context.Mentions.Add(mention);
                        }
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                return ApiResponse<Guid>.Success(comment.Id, "Comment updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<Guid>.Fail($"Error updating comment: {ex.Message}");
            }
        }
    }
}
