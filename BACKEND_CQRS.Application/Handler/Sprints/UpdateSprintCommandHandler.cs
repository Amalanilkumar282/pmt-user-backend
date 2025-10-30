using AutoMapper;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Sprints
{
    public class UpdateSprintCommandHandler : IRequestHandler<UpdateSprintCommand, ApiResponse<SprintDto>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UpdateSprintCommandHandler(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<SprintDto>> Handle(UpdateSprintCommand request, CancellationToken cancellationToken)
        {
            // Find existing sprint
            var sprint = await _context.Sprints
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (sprint == null)
            {
                return ApiResponse<SprintDto>.Fail($"Sprint with ID {request.Id} not found.");
            }

            // Update properties
            sprint.Name = request.SprintName ?? sprint.Name;
            sprint.SprintGoal = request.SprintGoal ?? sprint.SprintGoal;
            sprint.TeamId = request.TeamAssigned ?? sprint.TeamId;
            sprint.StartDate = request.StartDate ?? sprint.StartDate;
            sprint.DueDate = request.DueDate ?? sprint.DueDate;
            sprint.Status = request.Status ?? sprint.Status;
            sprint.StoryPoint = request.StoryPoint ?? sprint.StoryPoint;
            sprint.ProjectId = request.ProjectId ?? sprint.ProjectId;
            sprint.UpdatedAt = DateTimeOffset.UtcNow;

            _context.Sprints.Update(sprint);
            await _context.SaveChangesAsync(cancellationToken);

            var sprintDto = _mapper.Map<SprintDto>(sprint);
            return ApiResponse<SprintDto>.Success(sprintDto, "Sprint updated successfully");
        }
    }
}