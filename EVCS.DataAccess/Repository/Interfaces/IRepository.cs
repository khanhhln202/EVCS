using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.DataAccess.Repository.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetAsync(Expression<Func<T, bool>> filter, string? includeProps = null);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProps = null);
        Task AddAsync(T entity);
        void Remove(T entity);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
