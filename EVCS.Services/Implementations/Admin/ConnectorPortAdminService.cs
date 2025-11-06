using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVCS.DataAccess.Data;
using EVCS.Models.Entities;
using EVCS.Services.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.Interfaces.Admin
{
    public class ConnectorPortAdminService : IConnectorPortAdminService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public ConnectorPortAdminService(ApplicationDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }


        public async Task<IReadOnlyList<ConnectorPortUpsertDto>> GetByChargerAsync(Guid chargerId)
        => await _db.ConnectorPorts.AsNoTracking()
        .Where(c => c.ChargerId == chargerId && !c.IsDeleted)
        .ProjectTo<ConnectorPortUpsertDto>(_mapper.ConfigurationProvider)
        .ToListAsync();


        public async Task<ConnectorPortUpsertDto?> GetAsync(Guid id)
        => await _db.ConnectorPorts.AsNoTracking()
        .Where(c => c.Id == id && !c.IsDeleted)
        .ProjectTo<ConnectorPortUpsertDto>(_mapper.ConfigurationProvider)
        .FirstOrDefaultAsync();


        public async Task<Guid> CreateAsync(ConnectorPortUpsertDto dto)
        {
            var entity = _mapper.Map<ConnectorPort>(dto);
            entity.Id = Guid.NewGuid();
            await _db.ConnectorPorts.AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity.Id;
        }


        public async Task UpdateAsync(ConnectorPortUpsertDto dto)
        {
            var entity = await _db.ConnectorPorts.FirstOrDefaultAsync(s => s.Id == dto.Id && !s.IsDeleted)
            ?? throw new KeyNotFoundException("Port not found");
            _db.Entry(entity).Property(e => e.RowVersion).OriginalValue = dto.RowVersion ?? entity.RowVersion;
            _mapper.Map(dto, entity);
            await _db.SaveChangesAsync();
        }


        public async Task SoftDeleteAsync(Guid id)
        {
            var entity = await _db.ConnectorPorts.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted)
            ?? throw new KeyNotFoundException("Port not found");
            entity.IsDeleted = true; entity.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
