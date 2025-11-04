using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace BACKEND_CQRS.Application.Command
{
    public class CreateMessageCommand : IRequest<ApiResponse<MessageDto>>
    {
        [Required(ErrorMessage = "Channel ID is required")]
        public Guid ChannelId { get; set; }

        [Required(ErrorMessage = "Message body is required")]
        [MaxLength(5000, ErrorMessage = "Message body cannot exceed 5000 characters")]
        public string Body { get; set; }

        public int? MentionUserId { get; set; }

        [Required(ErrorMessage = "CreatedBy is required")]
        public int CreatedBy { get; set; }
    }
}