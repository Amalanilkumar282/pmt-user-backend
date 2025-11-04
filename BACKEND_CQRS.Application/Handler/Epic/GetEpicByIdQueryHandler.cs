using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Epic;
using BACKEND_CQRS.Application.Wrapper;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Epic
{
    public class GetEpicByIdQueryHandler : IRequestHandler<GetEpicByIdQuery, ApiResponse<EpicDto>>
    {
        private readonly IEpicRepository _epicRepository;
        private readonly IMapper _mapper;

        public GetEpicByIdQueryHandler(IEpicRepository epicRepository, IMapper mapper)
        {
            _epicRepository = epicRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<EpicDto>> Handle(GetEpicByIdQuery request, CancellationToken cancellationToken)
        {
            var epics = await _epicRepository.FindAsync(e => e.Id == request.EpicId);
            var epic = epics.FirstOrDefault();

            if (epic == null)
                return ApiResponse<EpicDto>.Fail("Epic not found");

            var epicDto = _mapper.Map<EpicDto>(epic);
            return ApiResponse<EpicDto>.Success(epicDto);
        }
    }
}
