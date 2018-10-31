using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CrossExchange
{
    public abstract class GenericRepository<T> : IGenericRepository<T>
        where T : class, new()
    {
        protected ExchangeContext _dbContext { get; set; }

        public async Task<T> GetAsync(int id)
        {
            return await _dbContext.FindAsync<T>(id);
        }

        public IQueryable<T> Query()
        {
            return _dbContext.Set<T>().AsQueryable();
        }

        public async Task InsertAsync(T entity)
        {
            _dbContext.Add(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbContext.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}