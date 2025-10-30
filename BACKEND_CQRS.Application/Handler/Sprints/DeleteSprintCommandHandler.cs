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
    public class DeleteSprintCommandHandler : IRequestHandler<DeleteSprintCommand, ApiResponse<bool>>
    {
        private readonly AppDbContext _context;

        public DeleteSprintCommandHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteSprintCommand request, CancellationToken cancellationToken)
        {
            var sprint = await _context.Sprints
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (sprint == null)
            {
                return ApiResponse<bool>.Fail($"Sprint with ID {request.Id} not found.");
            }

            _context.Sprints.Remove(sprint);
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Sprint deleted successfully");
        }
    }
}