using AutoMapper;
using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Query.Epic;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Handler.Epic
{
    public class GetEpicsByProjectIdQueryHandler : IRequestHandler<GetEpicsByProjectIdQuery, List<EpicDto>>
    {
        private readonly IEpicRepository _epicRepository;
        private readonly IMapper _mapper;

        public GetEpicsByProjectIdQueryHandler(IEpicRepository epicRepository, IMapper mapper)
        {
            _epicRepository = epicRepository;
            _mapper = mapper;
        }

        public async Task<List<EpicDto>> Handle(GetEpicsByProjectIdQuery request, CancellationToken cancellationToken)
        {
            var epics = await _epicRepository.GetEpicsByProjectIdAsync(request.ProjectId);
            return _mapper.Map<List<EpicDto>>(epics);
        }
    }
}
