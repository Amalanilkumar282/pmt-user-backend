using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Domain.Persistance
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> CreateAsync(T entity);

        Task<T> UpdateAsync(T entity);

        Task<bool> DeleteAsync(T entity);

        Task<List<T>> GetAllAsync();

        Task<T> GetByIdAsync(int id);

        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
    }
}
