using AutoMapper;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Epic
{
    public class UpdateEpicCommandHandler : IRequestHandler<UpdateEpicCommand, ApiResponse<Guid>>
    {
        private readonly IEpicRepository _epicRepository;
        private readonly IMapper _mapper;

        public UpdateEpicCommandHandler(IEpicRepository epicRepository, IMapper mapper)
        {
            _epicRepository = epicRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<Guid>> Handle(UpdateEpicCommand request, CancellationToken cancellationToken)
        {
            var epics = await _epicRepository.FindAsync(e => e.Id == request.Id);
            var epic = epics.FirstOrDefault();

            if (epic == null)
                return ApiResponse<Guid>.Fail("Epic not found");

            // Update only the fields that are provided
            if (!string.IsNullOrWhiteSpace(request.Title))
                epic.Title = request.Title;

            if (request.Description != null)
                epic.Description = request.Description;

            if (request.StartDate.HasValue)
                epic.StartDate = request.StartDate;

            if (request.DueDate.HasValue)
                epic.DueDate = request.DueDate;

            if (request.AssigneeId.HasValue)
                epic.AssigneeId = request.AssigneeId;

            if (request.ReporterId.HasValue)
                epic.ReporterId = request.ReporterId;

            if (request.Labels != null)
                epic.Labels = request.Labels;

            epic.UpdatedAt = DateTimeOffset.UtcNow;

            await _epicRepository.UpdateAsync(epic);
            return ApiResponse<Guid>.Success(epic.Id, "Epic updated successfully");
        }
    }
}
