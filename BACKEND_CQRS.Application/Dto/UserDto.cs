using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Dto
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public bool IsSuperAdmin { get; set; }
        public DateTimeOffset? LastLogin { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public int? Version { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string JiraId { get; set; }
        public string Type { get; set; }
    }
}
