using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Dto
{
    public class ActivityLogDto
    {
        public Guid Id { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public string EntityType { get; set; }
        public Guid EntityId { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
