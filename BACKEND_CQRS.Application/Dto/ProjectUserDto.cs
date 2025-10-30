using System;
using System.Collections.Generic;

namespace BACKEND_CQRS.Application.Dto
{
    public class ProjectUserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuperAdmin { get; set; }
        public string JiraId { get; set; }
        public string Type { get; set; }
        public DateTimeOffset? LastLogin { get; set; }
        
        // Project-specific fields
        public int? RoleId { get; set; }
        public string RoleName { get; set; }
        public bool? IsOwner { get; set; }
        public DateTimeOffset? AddedAt { get; set; }
        public int? AddedBy { get; set; }
        public string AddedByName { get; set; }
        
        // Teams the user is part of in this project
        public List<UserTeamDto> Teams { get; set; } = new List<UserTeamDto>();
    }

    public class UserTeamDto
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
    }
}