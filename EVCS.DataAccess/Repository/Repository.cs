using EVCS.DataAccess.Data;
using EVCS.DataAccess.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;


        public Repository(ApplicationDbContext db)
        {
            _db = db;
            dbSet = _db.Set<T>();
        }


        public async Task AddAsync(T entity) => await dbSet.AddAsync(entity);


        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null) query = query.Where(filter);
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(includeProp.Trim());
            }
            return await query.ToListAsync();
        }


        public async Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
        {
            IQueryable<T> query = tracked ? dbSet : dbSet.AsNoTracking();
            query = query.Where(filter);
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    query = query.Include(includeProp.Trim());
            }
            return await query.FirstOrDefaultAsync();
        }


        public void Remove(T entity) => dbSet.Remove(entity);
        public void RemoveRange(IEnumerable<T> entities) => dbSet.RemoveRange(entities);
    }
}
