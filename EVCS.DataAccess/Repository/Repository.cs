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
        private readonly DbSet<T> _set;
        public Repository(ApplicationDbContext db)
        {
            _db = db; _set = _db.Set<T>();
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? includeProps = null)
        {
            IQueryable<T> q = _set.Where(filter);
            if (!string.IsNullOrWhiteSpace(includeProps))
                foreach (var p in includeProps.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    q = q.Include(p.Trim());
            return await q.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProps = null)
        {
            IQueryable<T> q = _set;
            if (filter != null) q = q.Where(filter);
            if (!string.IsNullOrWhiteSpace(includeProps))
                foreach (var p in includeProps.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    q = q.Include(p.Trim());
            return await q.ToListAsync();
        }

        public async Task AddAsync(T entity) => await _set.AddAsync(entity);
        public void Remove(T entity) => _set.Remove(entity);
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
    }
}
