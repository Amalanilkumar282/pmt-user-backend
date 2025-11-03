using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BACKEND_CQRS.Application.Query.Project;

namespace BACKEND_CQRS.Application.Handler.Project
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
            // First get project IDs for the user to avoid correlated subquery issues
            var projectIds = await _dbContext.ProjectMembers
                .AsNoTracking()
                .Where(pm => pm.UserId == request.UserId)
                .Select(pm => pm.ProjectId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (projectIds == null || !projectIds.Any())
            {
                return ApiResponse<List<ProjectDto>>.Fail("No projects found for this user.");
            }

            // Query projects where user is a member, including related entities
            var projects = await _dbContext.Projects
                .AsNoTracking()
                .Include(p => p.ProjectManager)
                .Include(p => p.DeliveryUnit)
                .Include(p => p.Status)
                .Where(p => projectIds.Contains(p.Id))
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
