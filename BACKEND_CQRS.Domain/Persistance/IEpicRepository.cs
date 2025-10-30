using BACKEND_CQRS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Persistance
{
    public interface IEpicRepository
    {
        Task<List<Epic>> GetEpicsByProjectIdAsync(Guid projectId);
    }
}
