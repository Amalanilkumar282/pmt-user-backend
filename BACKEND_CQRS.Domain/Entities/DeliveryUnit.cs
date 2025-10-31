//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BACKEND_CQRS.Domain.Entities
//{
//    [Table("delivery_units")]
//    public class DeliveryUnit
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [MaxLength(200)]
//        [Column("name")]
//        public string Name { get; set; }

//        [MaxLength(50)]
//        [Column("code")]
//        public string? Code { get; set; }

//        [Column("description")]
//        public string? Description { get; set; }

//        [Column("manager_id")]
//        public int? ManagerId { get; set; }

//        [Column("is_active")]
//        public bool IsActive { get; set; } = true;

//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

//        [Column("updated_at")]
//        public DateTime? UpdatedAt { get; set; }

//        // Navigation properties
//        [ForeignKey("ManagerId")]
//        public Users? Manager { get; set; }

//        public ICollection<Projects> Projects { get; set; }
//    }
//}

using BACKEND_CQRS.Domain.Entities;
using System;

using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

using System.Linq;

using System.Text;

using System.Threading.Tasks;

namespace PmtAdmin.Domain.Entities

{

    [Table("delivery_units")]

    public class DeliveryUnit

    {

        [Key]

        [Column("id")]

        public int Id { get; set; }

        [Required]

        [MaxLength(200)]

        [Column("name")]

        public string Name { get; set; }

        [MaxLength(50)]

        [Column("code")]

        public string? Code { get; set; }

        [Column("description")]

        public string? Description { get; set; }

        [MaxLength(200)]

        [Column("du_head_name")]

        public string? DuHeadName { get; set; }

        [MaxLength(255)]

        [Column("du_head_email")]

        public string? DuHeadEmail { get; set; }

        [Column("is_active")]

        public bool IsActive { get; set; } = true;

        [Column("created_at")]

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties

        public ICollection<Projects> Projects { get; set; }

    }

}
