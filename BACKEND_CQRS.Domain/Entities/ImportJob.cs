//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BACKEND_CQRS.Domain.Entities
//{
//    [Table("import_jobs")]
//    public class ImportJob
//    {
//        [Key]
//        [Column("id")]
//        public int Id { get; set; }

//        [Required]
//        [MaxLength(50)]
//        [Column("type")]
//        public string Type { get; set; }

//        [MaxLength(50)]
//        [Column("source")]
//        public string? Source { get; set; }

//        [MaxLength(50)]
//        [Column("status")]
//        public string Status { get; set; } = "pending";

//        [Column("started_by")]
//        public int? StartedBy { get; set; }

//        [Column("started_at")]
//        public DateTime? StartedAt { get; set; }

//        [Column("finished_at")]
//        public DateTime? FinishedAt { get; set; }

//        [Column("summary", TypeName = "jsonb")]
//        public string? Summary { get; set; }

//        [Column("details", TypeName = "jsonb")]
//        public string? Details { get; set; }

//        [Column("created_at")]
//        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

//        // Navigation properties
//        [ForeignKey("StartedBy")]
//        public Users? StartedByUser { get; set; }
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
    [Table("import_jobs")]
    public class ImportJob
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("type")]
        public string Type { get; set; }

        [MaxLength(50)]
        [Column("source")]
        public string? Source { get; set; }

        [MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("started_by")]
        public int? StartedBy { get; set; }

        [Column("started_at")]
        public DateTime? StartedAt { get; set; }

        [Column("finished_at")]
        public DateTime? FinishedAt { get; set; }

        [Column("summary", TypeName = "jsonb")]
        public string? Summary { get; set; }

        [Column("details", TypeName = "jsonb")]
        public string? Details { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("StartedBy")]
        public Users? StartedByUser { get; set; }
    }

}