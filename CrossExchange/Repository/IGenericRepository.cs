using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CrossExchange
{
    public interface IGenericRepository<T>
    {
        Task<T> GetAsync(int id);

        IQueryable<T> Query();

        Task InsertAsync(T entity);

        Task UpdateAsync(T entity);
    }
}