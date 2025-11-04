using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace BACKEND_CQRS.Application.Command
{
    public class DeleteChannelCommand : IRequest<ApiResponse<bool>>
    {
        [Required(ErrorMessage = "Channel ID is required")]
        public Guid ChannelId { get; set; }

        /// <summary>
        /// Optional: User ID who is deleting the channel (for audit purposes)
        /// </summary>
        public int? DeletedBy { get; set; }

        public DeleteChannelCommand() { }

        public DeleteChannelCommand(Guid channelId, int? deletedBy = null)
        {
            ChannelId = channelId;
            DeletedBy = deletedBy;
        }
    }
}