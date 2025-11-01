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

namespace BACKEND_CQRS.Application.Handler.Epic
{
    public class CreateEpicCommandHandler : IRequestHandler<CreateEpicCommand, ApiResponse<CreateEpicDto>>
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CreateEpicCommandHandler(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<CreateEpicDto>> Handle(CreateEpicCommand request, CancellationToken cancellationToken)
        {
            var epic = _mapper.Map<Domain.Entities.Epic>(request);
            epic.Id = Guid.NewGuid();
            epic.CreatedAt = DateTimeOffset.UtcNow;
            epic.UpdatedAt = DateTimeOffset.UtcNow;

            _context.Epic.Add(epic);
            await _context.SaveChangesAsync(cancellationToken);

            var dto = _mapper.Map<CreateEpicDto>(epic);
            return ApiResponse<CreateEpicDto>.Created(dto, "Epic created successfully");
        }
    }
}
