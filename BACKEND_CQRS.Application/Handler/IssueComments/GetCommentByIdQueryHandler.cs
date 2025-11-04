using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.IssueComments;
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
    public class GetCommentByIdQueryHandler : IRequestHandler<GetCommentByIdQuery, ApiResponse<IssueCommentDto>>
    {
        private readonly AppDbContext _context;

        public GetCommentByIdQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<IssueCommentDto>> Handle(GetCommentByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var comment = await _context.IssueComments
                    .Include(c => c.Author)
                    .Include(c => c.Mentions)
                        .ThenInclude(m => m.MentionedUser)
                    .Where(c => c.Id == request.Id)
                    .Select(c => new IssueCommentDto
                    {
                        Id = c.Id,
                        IssueId = c.IssueId.Value,
                        AuthorId = c.AuthorId,
                        AuthorName = c.Author.Name,
                        AuthorAvatarUrl = c.Author.AvatarUrl,
                        Body = c.Body,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        Mentions = c.Mentions.Select(m => new MentionDto
                        {
                            Id = m.Id,
                            MentionUserId = m.MentionUserId.Value,
                            MentionUserName = m.MentionedUser != null ? m.MentionedUser.Name : null,
                            MentionUserEmail = m.MentionedUser != null ? m.MentionedUser.Email : null
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (comment == null)
                {
                    return ApiResponse<IssueCommentDto>.Fail("Comment not found");
                }

                return ApiResponse<IssueCommentDto>.Success(comment, "Comment retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<IssueCommentDto>.Fail($"Error retrieving comment: {ex.Message}");
            }
        }
    }
}
