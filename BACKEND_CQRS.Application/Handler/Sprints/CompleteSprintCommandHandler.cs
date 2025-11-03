using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Sprints
{
    public class CompleteSprintCommandHandler : IRequestHandler<CompleteSprintCommand, ApiResponse<bool>>
    {
        private readonly AppDbContext _dbContext;

        public CompleteSprintCommandHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<bool>> Handle(CompleteSprintCommand request, CancellationToken cancellationToken)
        {
            // Find the sprint
            var sprint = await _dbContext.Sprints
                .FirstOrDefaultAsync(s => s.Id == request.SprintId, cancellationToken);

            if (sprint == null)
            {
                return ApiResponse<bool>.Fail($"Sprint with ID {request.SprintId} not found.");
            }

            // Check if already completed
            if (sprint.Status?.ToUpper() == "COMPLETED")
            {
                return ApiResponse<bool>.Success(true, "Sprint is already completed.");
            }

            // Update status to COMPLETED
            sprint.Status = "COMPLETED";
            sprint.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, $"Sprint '{sprint.Name}' has been marked as completed successfully.");
        }
    }
}