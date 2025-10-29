using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Application.Dto
{
    public class TeamCountDto
    {
        public int TotalTeams { get; set; }
        public int ActiveTeams { get; set; }
    }
}
