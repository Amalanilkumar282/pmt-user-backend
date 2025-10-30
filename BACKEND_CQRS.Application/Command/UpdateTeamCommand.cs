using BACKEND_CQRS.Application.Dto;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Command
{
    public class UpdateTeamCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public UpdateTeamDto Team { get; set; }

        public UpdateTeamCommand(int id, UpdateTeamDto team)
        {
            Id = id;
            Team = team;
        }
    }
}
