using BACKEND_CQRS.Application.Dto;
using BACKEND_CQRS.Application.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Query
{
    public class GetChannelsByTeamIdQuery : IRequest<ApiResponse<List<ChannelDto>>>
    {
        public int TeamId { get; }

        public GetChannelsByTeamIdQuery(int teamId)
        {
            TeamId = teamId;
        }
    }
}
