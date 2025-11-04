using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Channels
{
    public class DeleteChannelCommandHandler : IRequestHandler<DeleteChannelCommand, ApiResponse<bool>>
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<DeleteChannelCommandHandler> _logger;

        public DeleteChannelCommandHandler(AppDbContext dbContext, ILogger<DeleteChannelCommandHandler> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponse<bool>> Handle(DeleteChannelCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Deleting channel {ChannelId} requested by user {DeletedBy}",
                    request.ChannelId, request.DeletedBy);

                // Find the channel
                var channel = await _dbContext.Channels
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.Id == request.ChannelId, cancellationToken);

                if (channel == null)
                {
                    _logger.LogWarning("Channel {ChannelId} not found", request.ChannelId);
                    return ApiResponse<bool>.Fail($"Channel with ID {request.ChannelId} does not exist.");
                }

                // Get message count for logging
                var messageCount = channel.Messages?.Count ?? 0;

                // Remove all messages in the channel first
                if (channel.Messages != null && channel.Messages.Any())
                {
                    _dbContext.Messages.RemoveRange(channel.Messages);
                    _logger.LogInformation(
                        "Removing {Count} messages from channel {ChannelId}",
                        messageCount, request.ChannelId);
                }

                // Remove the channel
                _dbContext.Channels.Remove(channel);
                
                // Save changes
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Channel {ChannelId} ('{ChannelName}') and {MessageCount} messages deleted successfully by user {DeletedBy}",
                    request.ChannelId, channel.Name, messageCount, request.DeletedBy);

                return ApiResponse<bool>.Success(
                    true,
                    $"Channel '{channel.Name}' and {messageCount} message(s) deleted successfully.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting channel {ChannelId}", request.ChannelId);
                return ApiResponse<bool>.Fail(
                    "Failed to delete channel due to database constraints. Please ensure no other entities depend on this channel.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting channel {ChannelId}", request.ChannelId);
                return ApiResponse<bool>.Fail(
                    "An unexpected error occurred while deleting the channel. Please try again later.");
            }
        }
    }
}