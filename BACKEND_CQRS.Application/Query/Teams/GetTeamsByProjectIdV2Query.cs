using BACKEND_CQRS.Application.Dto;
using MediatR;
using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Query.Teams
{
    public record GetTeamsByProjectIdV2Query(Guid ProjectId) : IRequest<List<TeamDetailsV2Dto>>;
}
