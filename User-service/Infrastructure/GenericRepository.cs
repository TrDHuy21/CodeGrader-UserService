using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly USContext _USContext;

        public GenericRepository(USContext uSContext)
        {
            _USContext = uSContext;
        }

        public async Task AddAsync(T entity)
        {
           await _USContext.Set<T>().AddAsync(entity);
        }

        public Task DeleteAsync(T entity)
        {
            _USContext.Set<T>().Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _USContext.Set<T>().ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
          return await _USContext.Set<T>().FindAsync(id);               
        }

        public Task UpdateAsync(T entity)
        {
            _USContext.Entry(entity).State = EntityState.Modified;
            return Task.CompletedTask;
        }
    }
}
