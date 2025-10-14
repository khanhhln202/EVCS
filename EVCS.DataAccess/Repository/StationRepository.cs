using EVCS.DataAccess.Data;
using EVCS.DataAccess.Repository.Interfaces;
using EVCS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.DataAccess.Repository
{
    public class StationRepository : IStationRepository
    {
        private readonly ApplicationDbContext _db;
        public StationRepository(ApplicationDbContext db) => _db = db;

        public async Task AddAsync(Station e) => await _db.Stations.AddAsync(e);

        public async Task<Station?> GetAsync(Guid id) =>
            await _db.Stations.Include(s => s.Chargers.Where(c => !c.IsDeleted))
                              .ThenInclude(c => c.Ports.Where(p => !p.IsDeleted))
                              .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        public async Task<IEnumerable<Station>> GetAllAsync(Expression<Func<Station, bool>>? filter = null, params Expression<Func<Station, object>>[] includes)
        {
            IQueryable<Station> q = _db.Stations.Where(s => !s.IsDeleted);
            if (filter != null) q = q.Where(filter);
            foreach (var inc in includes) q = q.Include(inc);
            return await q.ToListAsync();
        }

        public void Update(Station e) => _db.Stations.Update(e);
        public void Remove(Station e) => _db.Stations.Remove(e);

        public Task<bool> ExistsCodeAsync(string code, Guid? excludeId = null) =>
            _db.Stations.AnyAsync(s => s.Code == code && !s.IsDeleted && (excludeId == null || s.Id != excludeId));
    }
}
