using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Sprints;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Sprints
{
    public class GetSprintsByTeamIdQueryHandler
        : IRequestHandler<GetSprintsByTeamIdQuery, ApiResponse<List<SprintDto>>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public GetSprintsByTeamIdQueryHandler(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<List<SprintDto>>> Handle(GetSprintsByTeamIdQuery request, CancellationToken cancellationToken)
        {
            var sprints = await _context.Sprints
                .Where(s => s.TeamId == request.TeamId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);

            if (sprints == null || !sprints.Any())
            {
                return ApiResponse<List<SprintDto>>.Fail("No sprints found for the specified team.");
            }

            var sprintDtos = _mapper.Map<List<SprintDto>>(sprints);
            return ApiResponse<List<SprintDto>>.Success(sprintDtos);
        }
    }
}