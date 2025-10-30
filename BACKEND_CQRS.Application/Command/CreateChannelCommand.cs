using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace BACKEND_CQRS.Application.Command
{
    public class CreateChannelCommand : IRequest<ApiResponse<ChannelDto>>
    {
        [Required]
        public int TeamId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ChannelName { get; set; }
    }
}