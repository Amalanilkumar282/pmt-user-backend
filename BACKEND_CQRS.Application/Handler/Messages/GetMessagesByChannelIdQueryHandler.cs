using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Messages;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Messages
{
    public class GetMessagesByChannelIdQueryHandler
        : IRequestHandler<GetMessagesByChannelIdQuery, ApiResponse<List<MessageDto>>>
    {
        private readonly AppDbContext _dbContext;

        public GetMessagesByChannelIdQueryHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<List<MessageDto>>> Handle(GetMessagesByChannelIdQuery request, CancellationToken cancellationToken)
        {
            var messages = await _dbContext.Messages
                .Include(m => m.MentionedUser)
                .Include(m => m.Creator)
                .Include(m => m.Updater)
                .Where(m => m.ChannelId == request.ChannelId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(request.Take)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    ChannelId = m.ChannelId,
                    Body = m.Body,
                    MentionUserId = m.MentionUserId,
                    MentionedUserName = m.MentionedUser != null ? m.MentionedUser.Name : null,
                    CreatedBy = m.CreatedBy,
                    CreatorName = m.Creator != null ? m.Creator.Name : null,
                    UpdatedBy = m.UpdatedBy,
                    UpdaterName = m.Updater != null ? m.Updater.Name : null,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (messages == null || !messages.Any())
            {
                return ApiResponse<List<MessageDto>>.Fail("No messages found for this channel.");
            }

            return ApiResponse<List<MessageDto>>.Success(messages);
        }
    }
}
