using AutoMapper;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Channels
{
    public class CreateChannelCommandHandler : IRequestHandler<CreateChannelCommand, ApiResponse<ChannelDto>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CreateChannelCommandHandler(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<ChannelDto>> Handle(CreateChannelCommand request, CancellationToken cancellationToken)
        {
            // Validate that the team exists
            var teamExists = await _context.Teams
                .AnyAsync(t => t.Id == request.TeamId, cancellationToken);

            if (!teamExists)
            {
                return ApiResponse<ChannelDto>.Fail($"Team with ID {request.TeamId} not found.");
            }

            // Create new channel
            var channel = new Channel
            {
                Id = Guid.NewGuid(),
                TeamId = request.TeamId,
                Name = request.ChannelName
            };

            _context.Channels.Add(channel);
            await _context.SaveChangesAsync(cancellationToken);

            // Map to DTO
            var channelDto = _mapper.Map<ChannelDto>(channel);
            return ApiResponse<ChannelDto>.Created(channelDto, "Channel created successfully");
        }
    }
}