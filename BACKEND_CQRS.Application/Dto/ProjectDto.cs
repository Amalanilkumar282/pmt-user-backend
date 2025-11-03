using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Dto
{
    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CustomerOrgName { get; set; }
        public string CustomerDomainUrl { get; set; }
        public string CustomerDescription { get; set; }
        public string PocEmail { get; set; }
        public string PocPhone { get; set; }
        public string ProjectManagerName { get; set; }
        public string DeliveryUnitName { get; set; }
        public string StatusName { get; set; }
    }
}
