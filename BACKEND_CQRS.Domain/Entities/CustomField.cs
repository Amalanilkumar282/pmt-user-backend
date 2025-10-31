//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BACKEND_CQRS.Domain.Entities
//{

//    [Table("custom_fields")]
//    public class CustomField
//    {
//        [Key]
//        [Column("projectid")]
//        public int ProjectId { get; set; }

//        [Column("name")]
//        public string? Name { get; set; }

//        [Column("value")]
//        public string? Value { get; set; }
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

    [Table("custom_fields")]
    public class CustomField
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("project_id")]
        public Guid ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public Projects? Project { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("value")]
        public string? Value { get; set; }
    }

}
