using AutoMapper;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Messages
{
    public class CreateMessageCommandHandler : IRequestHandler<CreateMessageCommand, ApiResponse<MessageDto>>
    {
        private readonly AppDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateMessageCommandHandler> _logger;

        public CreateMessageCommandHandler(
            AppDbContext dbContext,
            IMapper mapper,
            ILogger<CreateMessageCommandHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<MessageDto>> Handle(CreateMessageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating message in channel {ChannelId} by user {UserId}",
                    request.ChannelId, request.CreatedBy);

                // Validate that the channel exists
                var channel = await _dbContext.Channels
                    .FirstOrDefaultAsync(c => c.Id == request.ChannelId, cancellationToken);

                if (channel == null)
                {
                    _logger.LogWarning("Channel {ChannelId} not found", request.ChannelId);
                    return ApiResponse<MessageDto>.Fail($"Channel with ID {request.ChannelId} does not exist");
                }

                // Validate that the creator exists and is active
                var creator = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Id == request.CreatedBy, cancellationToken);

                if (creator == null)
                {
                    _logger.LogWarning("User {UserId} not found", request.CreatedBy);
                    return ApiResponse<MessageDto>.Fail($"User with ID {request.CreatedBy} does not exist");
                }

                if (creator.IsActive == false || creator.DeletedAt.HasValue)
                {
                    _logger.LogWarning("User {UserId} is not active or deleted", request.CreatedBy);
                    return ApiResponse<MessageDto>.Fail("User is not active or has been deleted");
                }

                // Validate mentioned user if provided
                if (request.MentionUserId.HasValue)
                {
                    var mentionedUser = await _dbContext.Users
                        .FirstOrDefaultAsync(u => u.Id == request.MentionUserId.Value, cancellationToken);

                    if (mentionedUser == null)
                    {
                        _logger.LogWarning("Mentioned user {UserId} not found", request.MentionUserId);
                        return ApiResponse<MessageDto>.Fail($"Mentioned user with ID {request.MentionUserId} does not exist");
                    }
                }

                // Create the message entity
                var message = new Message
                {
                    Id = Guid.NewGuid(),
                    ChannelId = request.ChannelId,
                    Body = request.Body,
                    MentionUserId = request.MentionUserId,
                    CreatedBy = request.CreatedBy,
                    UpdatedBy = request.CreatedBy,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                // Save to database
                await _dbContext.Messages.AddAsync(message, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Message {MessageId} created successfully in channel {ChannelId} by user {UserId}",
                    message.Id, request.ChannelId, request.CreatedBy);

                // Load navigation properties for DTO mapping
                var createdMessage = await _dbContext.Messages
                    .Include(m => m.Channel)
                    .Include(m => m.Creator)
                    .Include(m => m.MentionedUser)
                    .FirstOrDefaultAsync(m => m.Id == message.Id, cancellationToken);

                var messageDto = _mapper.Map<MessageDto>(createdMessage);

                return ApiResponse<MessageDto>.Success(
                    messageDto,
                    "Message created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating message in channel {ChannelId}", request.ChannelId);
                return ApiResponse<MessageDto>.Fail("An error occurred while creating the message. Please try again.");
            }
        }
    }
}