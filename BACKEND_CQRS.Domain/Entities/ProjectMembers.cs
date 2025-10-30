using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Entities
{
    [Table("project_members")]
    public class ProjectMembers
    {
        
        
            [Key]
            [Column("id")]
            public int Id { get; set; }

            [Column("project_id")]
            public Guid ProjectId { get; set; }

            
            [Column("user_id")]
            public int? UserId { get; set; }

            [Column("role_id")]
            public int? RoleId { get; set; }

            

            [Column("is_owner")]
            public bool? IsOwner { get; set; }

            [Column("added_at")]
            public DateTimeOffset? AddedAt { get; set; }

            [Column("added_by")]
            public int? AddedBy { get; set; }

        [ForeignKey("UserId")]
        public Users? User { get; set; }  // ✅ Add this navigation property

        [ForeignKey("ProjectId")]
        public Projects? Project { get; set; }
        [ForeignKey("RoleId")]
        public Role? Role { get; set; }


        [ForeignKey("RoleId")]
        public Role? Role { get; set; }  // ✅ Add this line


    }
    }


