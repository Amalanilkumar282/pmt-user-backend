using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using FRONTEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler
{
    public class GetUserProjectsQueryHandler : IRequestHandler<GetUserProjectsQuery, ApiResponse<List<ProjectDto>>>
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;

        public GetUserProjectsQueryHandler(IMapper mapper, AppDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<List<ProjectDto>>> Handle(GetUserProjectsQuery request, CancellationToken cancellationToken)
        {
            // Query projects where user is a member
            var projects = await _dbContext.Projects
                .Where(p => _dbContext.ProjectMembers
                                       .Any(pm => pm.ProjectId == p.Id && pm.UserId == request.UserId))
                .ToListAsync(cancellationToken);

            if (projects == null || !projects.Any())
            {
                return ApiResponse<List<ProjectDto>>.Fail("No projects found for this user.");
            }

            // Map entities to DTOs
            var projectDtos = _mapper.Map<List<ProjectDto>>(projects);
            return ApiResponse<List<ProjectDto>>.Success(projectDtos);
        }
    }
}
