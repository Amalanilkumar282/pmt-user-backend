using AutoMapper;
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

namespace BACKEND_CQRS.Application.Handler.Projects
{
    public class GetRecentProjectsQueryHandler : IRequestHandler<GetRecentProjectsQuery, ApiResponse<List<ProjectDto>>>
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _dbContext;

        public GetRecentProjectsQueryHandler(IMapper mapper, AppDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<List<ProjectDto>>> Handle(GetRecentProjectsQuery request, CancellationToken cancellationToken)
        {
            // Query recent projects ordered by UpdatedAt descending, exclude deleted
            var projects = await _dbContext.Projects
                .Where(p => p.DeletedAt == null)
                .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                .Take(request.Take)
                .ToListAsync(cancellationToken);

            if (projects == null || !projects.Any())
            {
                return ApiResponse<List<ProjectDto>>.Fail("No recent projects found.");
            }

            // Map entities to DTOs
            var projectDtos = _mapper.Map<List<ProjectDto>>(projects);
            return ApiResponse<List<ProjectDto>>.Success(projectDtos);
        }
    }
    }
