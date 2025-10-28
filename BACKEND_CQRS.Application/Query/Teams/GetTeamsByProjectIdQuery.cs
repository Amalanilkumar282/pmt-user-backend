using BACKEND_CQRS.Application.Dto;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Query.Teams
{
    public class GetTeamsByProjectIdQuery : IRequest<List<TeamDto>>
    {
        public Guid ProjectId { get; }

        public GetTeamsByProjectIdQuery(Guid projectId)
        {
            ProjectId = projectId;
        }
    }
}
