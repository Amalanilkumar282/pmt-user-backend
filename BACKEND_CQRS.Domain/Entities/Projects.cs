using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Entities
{
    [Table("projects")]
    public class Projects
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        [Required]
        public string Name { get; set; }

        [Column("key")]
        public string Key { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("customer_org_name")]
        public string CustomerOrgName { get; set; }

        [Column("customer_domain_url")]
        public string CustomerDomainUrl { get; set; }

        [Column("customer_description")]
        public string CustomerDescription { get; set; }

        [Column("poc_email")]
        public string PocEmail { get; set; }

        [Column("poc_phone")]
        public string PocPhone { get; set; }

        [Column("project_manager_id")]
        public int? ProjectManagerId { get; set; }

        [Column("project_manager_role_id")]
        public int? ProjectManagerRoleId { get; set; }

        [Column("status_id")]
        public int? StatusId { get; set; }

        [Column("delivery_unit_id")]
        public int? DeliveryUnitId { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [Column("metadata", TypeName = "jsonb")]
        public string Metadata { get; set; }

        [Column("deleted_at")]
        public DateTimeOffset? DeletedAt { get; set; }

        [Column("isimportedfromjira")]
        public bool? IsImportedFromJira { get; set; }

            [Column("template_id")]
            public int? TemplateId { get; set; }
    }
    }
}

