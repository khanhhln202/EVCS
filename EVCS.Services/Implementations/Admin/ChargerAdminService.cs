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
    public class ChargerAdminService : IChargerAdminService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public ChargerAdminService(ApplicationDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }


        public async Task<IReadOnlyList<ChargerUnitUpsertDto>> GetByStationAsync(Guid stationId)
        => await _db.ChargerUnits.AsNoTracking()
        .Where(c => c.StationId == stationId && !c.IsDeleted)
        .ProjectTo<ChargerUnitUpsertDto>(_mapper.ConfigurationProvider)
        .ToListAsync();


        public async Task<ChargerUnitUpsertDto?> GetAsync(Guid id)
        => await _db.ChargerUnits.AsNoTracking()
        .Where(c => c.Id == id && !c.IsDeleted)
        .ProjectTo<ChargerUnitUpsertDto>(_mapper.ConfigurationProvider)
        .FirstOrDefaultAsync();


        public async Task<Guid> CreateAsync(ChargerUnitUpsertDto dto)
        {
            var entity = _mapper.Map<ChargerUnit>(dto);
            entity.Id = Guid.NewGuid();
            await _db.ChargerUnits.AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity.Id;
        }


        public async Task UpdateAsync(ChargerUnitUpsertDto dto)
        {
            var entity = await _db.ChargerUnits.FirstOrDefaultAsync(s => s.Id == dto.Id && !s.IsDeleted)
            ?? throw new KeyNotFoundException("Charger not found");
            _db.Entry(entity).Property(e => e.RowVersion).OriginalValue = dto.RowVersion ?? entity.RowVersion;
            _mapper.Map(dto, entity);
            await _db.SaveChangesAsync();
        }


        public async Task SoftDeleteAsync(Guid id)
        {
            var entity = await _db.ChargerUnits.FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted)
            ?? throw new KeyNotFoundException("Charger not found");
            entity.IsDeleted = true; entity.DeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
