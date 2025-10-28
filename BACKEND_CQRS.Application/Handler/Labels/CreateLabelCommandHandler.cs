using AutoMapper;
using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Labels
{
    public class CreateLabelCommandHandler : IRequestHandler<CreateLabelCommand, ApiResponse<int>>
    {
        private readonly ILabelRepository _labelRepository;
        private readonly IMapper _mapper;

        public CreateLabelCommandHandler(ILabelRepository labelRepository, IMapper mapper)
        {
            _labelRepository = labelRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<int>> Handle(CreateLabelCommand request, CancellationToken cancellationToken)
        {
            var label = _mapper.Map<Label>(request);
            var createdLabel = await _labelRepository.AddLabelAsync(label);
            return ApiResponse<int>.Created(createdLabel.Id, "Label created successfully");
        }
    }
}
