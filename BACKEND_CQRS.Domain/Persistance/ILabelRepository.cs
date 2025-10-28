using BACKEND_CQRS.Domain.Entities;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Persistance
{
    public interface ILabelRepository : IGenericRepository<Label>
    {
        // Custom query methods specific to Label
        Task<Label> AddLabelAsync(Label label);
    }
}
