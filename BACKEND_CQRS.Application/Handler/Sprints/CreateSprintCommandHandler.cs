using AutoMapper;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Sprints
{
    public class CreateSprintCommandHandler : IRequestHandler<CreateSprintCommand, ApiResponse<CreateSprintDto>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CreateSprintCommandHandler(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<CreateSprintDto>> Handle(CreateSprintCommand request, CancellationToken cancellationToken)
        {
            var sprint = _mapper.Map<Sprint>(request);
            sprint.Id = Guid.NewGuid(); // Auto-generate ID
            sprint.Name = request.SprintName;
            sprint.TeamId = request.TeamAssigned;
            sprint.ProjectId = request.ProjectId;
            sprint.CreatedAt = DateTimeOffset.UtcNow;

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<CreateSprintDto>(sprint);
            return ApiResponse<CreateSprintDto>.Created(dto, "Sprint created successfully");
        }
    }
}
