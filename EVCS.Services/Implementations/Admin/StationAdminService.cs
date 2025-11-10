using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVCS.DataAccess.Data;
using EVCS.Models.Entities;
using EVCS.Models.Enums;
using EVCS.Services.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.Interfaces.Admin
{
    public class StationAdminService : IStationAdminService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;


        public StationAdminService(ApplicationDbContext db, IMapper mapper)
        { _db = db; _mapper = mapper; }


        public async Task<IReadOnlyList<StationListAdminDto>> GetAllAsync(string? city = null)
        {
            var q = _db.Stations.AsNoTracking().Where(s => !s.IsDeleted);
            if (!string.IsNullOrWhiteSpace(city)) q = q.Where(s => s.City == city);


            return await q
            .Select(s => new StationListAdminDto(
            s.Id, s.Code, s.Name, s.City,
            s.Status == StationStatus.Online,
            _db.ChargerUnits.Count(c => c.StationId == s.Id && !c.IsDeleted),
            _db.ConnectorPorts.Count(p => !p.IsDeleted && _db.ChargerUnits.Any(c => c.Id == p.ChargerId && c.StationId == s.Id && !c.IsDeleted))
            ))
            .ToListAsync();
        }


        public async Task<StationUpsertDto?> GetAsync(Guid id)
        => await _db.Stations.AsNoTracking()
        .Where(s => s.Id == id && !s.IsDeleted)
        .ProjectTo<StationUpsertDto>(_mapper.ConfigurationProvider)
        .FirstOrDefaultAsync();


        public async Task<Guid> CreateAsync(StationUpsertDto dto)
        {
            var entity = _mapper.Map<Station>(dto);
            entity.Id = Guid.NewGuid();
            await _db.Stations.AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity.Id;
        }


        public async Task UpdateAsync(StationUpsertDto dto)
        {
            var entity = await _db.Stations.FirstOrDefaultAsync(s => s.Id == dto.Id && !s.IsDeleted)
            ?? throw new KeyNotFoundException("Station not found");
            _db.Entry(entity).Property(e => e.RowVersion).OriginalValue = dto.RowVersion ?? entity.RowVersion;
            _mapper.Map(dto, entity);
            await _db.SaveChangesAsync();
        }


        public async Task SoftDeleteAsync(Guid id)
        {
            var entity = await _db.Stations.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted)
                ?? throw new KeyNotFoundException("Station not found");
            entity.IsDeleted = true; entity.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
