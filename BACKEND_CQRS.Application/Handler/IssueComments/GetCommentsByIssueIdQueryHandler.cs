using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.IssueComments;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.IssueComments
{
    public class GetCommentsByIssueIdQueryHandler : IRequestHandler<GetCommentsByIssueIdQuery, ApiResponse<List<IssueCommentDto>>>
    {
        private readonly AppDbContext _context;

        public GetCommentsByIssueIdQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<IssueCommentDto>>> Handle(GetCommentsByIssueIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var comments = await _context.IssueComments
                    .Include(c => c.Author)
                    .Include(c => c.Mentions)
                        .ThenInclude(m => m.MentionedUser)
                    .Where(c => c.IssueId == request.IssueId)
                    .OrderByDescending(c => c.CreatedAt)
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
                    .ToListAsync(cancellationToken);

                return ApiResponse<List<IssueCommentDto>>.Success(comments, "Comments retrieved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<IssueCommentDto>>.Fail($"Error retrieving comments: {ex.Message}");
            }
        }
    }
}
