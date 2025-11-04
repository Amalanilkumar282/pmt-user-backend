using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.ProjectMembers
{
    public class DeleteProjectMemberCommandHandler : IRequestHandler<DeleteProjectMemberCommand, ApiResponse<string>>
    {
        private readonly IGenericRepository<Domain.Entities.ProjectMembers> _projectMemberRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<DeleteProjectMemberCommandHandler> _logger;

        public DeleteProjectMemberCommandHandler(
            IGenericRepository<Domain.Entities.ProjectMembers> projectMemberRepository,
            AppDbContext context,
            ILogger<DeleteProjectMemberCommandHandler> logger)
        {
            _projectMemberRepository = projectMemberRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<string>> Handle(DeleteProjectMemberCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting user {UserId} from project {ProjectId}", request.UserId, request.ProjectId);

                var member = await _context.ProjectMembers
                    .FirstOrDefaultAsync(pm => pm.ProjectId == request.ProjectId && pm.UserId == request.UserId, cancellationToken);

                if (member == null)
                {
                    return ApiResponse<string>.Fail("Project member not found.");
                }

                await _projectMemberRepository.DeleteAsync(member);

                _logger.LogInformation("Successfully removed user {UserId} from project {ProjectId}", request.UserId, request.ProjectId);

                return ApiResponse<string>.Success("Member removed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing member {UserId} from project {ProjectId}", request.UserId, request.ProjectId);
                return ApiResponse<string>.Fail("An error occurred while removing the project member.");
            }
        }
    }
}
