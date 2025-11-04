using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
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
    public class CreateIssueCommentCommandHandler : IRequestHandler<CreateIssueCommentCommand, ApiResponse<CreateIssueCommentDto>>
    {
        private readonly AppDbContext _context;

        public CreateIssueCommentCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<CreateIssueCommentDto>> Handle(CreateIssueCommentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate if issue exists
                var issueExists = await _context.Issues.AnyAsync(i => i.Id == request.IssueId, cancellationToken);
                if (!issueExists)
                {
                    return ApiResponse<CreateIssueCommentDto>.Fail("Issue not found");
                }

                // Validate if author exists
                var authorExists = await _context.Users.AnyAsync(u => u.Id == request.AuthorId, cancellationToken);
                if (!authorExists)
                {
                    return ApiResponse<CreateIssueCommentDto>.Fail("Author not found");
                }

                // Create the comment
                var comment = new IssueComment
                {
                    Id = Guid.NewGuid(),
                    IssueId = request.IssueId,
                    AuthorId = request.AuthorId,
                    MentionId = request.AuthorId, // Set default mention to author
                    Body = request.Body,
                    CreatedBy = request.AuthorId,
                    UpdatedBy = request.AuthorId,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                _context.IssueComments.Add(comment);

                // Create mentions if any
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
                                CreatedBy = request.AuthorId,
                                UpdatedBy = request.AuthorId,
                                CreatedAt = DateTimeOffset.UtcNow,
                                UpdatedAt = DateTimeOffset.UtcNow
                            };
                            _context.Mentions.Add(mention);
                        }
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);

                var dto = new CreateIssueCommentDto
                {
                    Id = comment.Id,
                    IssueId = comment.IssueId.Value,
                    Body = comment.Body,
                    CreatedAt = comment.CreatedAt
                };

                return ApiResponse<CreateIssueCommentDto>.Created(dto, "Comment created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CreateIssueCommentDto>.Fail($"Error creating comment: {ex.Message}");
            }
        }
    }
}
