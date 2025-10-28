using BACKEND_CQRS.Application.Command;
using BACKEND_CQRS.Domain.Persistance;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BACKEND_CQRS.Domain.Entities;


namespace BACKEND_CQRS.Application.Handlers.TeamHandlers


{
    public class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, int>
    {
        private readonly ITeamRepository _teamRepository;

        public CreateTeamCommandHandler(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        public async Task<int> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
        {
            // 1️⃣ Create Team entity
            var team = new Teams
            {
                ProjectId = request.ProjectId,
                Name = request.Name,
                Description = request.Description,
                LeadId = request.LeadId,
                Label = request.Label,
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // 2️⃣ Save to DB (and get ID)
            var teamId = await _teamRepository.CreateTeamAsync(team);

            // 3️⃣ Add team members (including lead)
            var allMembers = new List<int>(request.MemberIds ?? new());
            if (request.LeadId.HasValue && !allMembers.Contains(request.LeadId.Value))
                allMembers.Add(request.LeadId.Value);

            await _teamRepository.AddMembersAsync(teamId, allMembers);

            return teamId;
        }
    }

}
