using AutoMapper;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using BACKEND_CQRS.Infrastructure.Repository;
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
    public class UpdateProjectMemberCommandHandler
        : IRequestHandler<UpdateProjectMemberCommand, ApiResponse<UpdateProjectMemberResponseDto>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UpdateProjectMemberCommandHandler(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<UpdateProjectMemberResponseDto>> Handle(
            UpdateProjectMemberCommand request,
            CancellationToken cancellationToken)
        {
            // Fetch project member using ID
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (member == null)
                return ApiResponse<UpdateProjectMemberResponseDto>.Fail("Project member not found.");

            // ✅ Only update the RoleId
            member.RoleId = request.RoleId;

            await _context.SaveChangesAsync(cancellationToken);

            // Get role name from Roles table
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

            var response = new UpdateProjectMemberResponseDto
            {
                Id = member.Id,
                RoleId = member.RoleId,
                Role = role?.Name
            };

            return ApiResponse<UpdateProjectMemberResponseDto>.Success(response, "Role updated successfully.");
        }
    }
    }
