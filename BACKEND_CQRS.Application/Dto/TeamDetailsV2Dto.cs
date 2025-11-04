using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Dto
{
    /// <summary>
    /// Simplified team details DTO for sprint planning - properly handles nullable emails
    /// </summary>
    public class TeamDetailsV2Dto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<TeamMemberV2Dto> Members { get; set; } = new();

        public class TeamMemberV2Dto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Email { get; set; } // Nullable email field
            public string Role { get; set; } = string.Empty;
        }
    }
}
