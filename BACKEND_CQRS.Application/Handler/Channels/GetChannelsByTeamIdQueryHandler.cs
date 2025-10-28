using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Channels
{
    public class GetChannelsByTeamIdQueryHandler : IRequestHandler<GetChannelsByTeamIdQuery, ApiResponse<List<ChannelDto>>>
    {
        private readonly AppDbContext _dbContext;

        public GetChannelsByTeamIdQueryHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<List<ChannelDto>>> Handle(GetChannelsByTeamIdQuery request, CancellationToken cancellationToken)
        {
            var channels = await _dbContext.Channels
                .Include(c => c.Team)
                .Where(c => c.TeamId == request.TeamId)
                .Select(c => new ChannelDto
                {
                    Id = c.Id,
                    Name = c.Name,
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (channels == null || !channels.Any())
            {
                return ApiResponse<List<ChannelDto>>.Fail("No channels found for this team.");
            }

            return ApiResponse<List<ChannelDto>>.Success(channels);
        }
    }
}
