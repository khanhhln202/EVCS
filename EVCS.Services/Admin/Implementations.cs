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

namespace EVCS.Services.Admin
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
    public class BookingPolicyService : IBookingPolicyService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public BookingPolicyService(ApplicationDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }


        public async Task<BookingPolicyDto> GetCurrentAsync()
        {
            var policy = await _db.BookingPolicies.AsNoTracking().OrderBy(x => x.CreatedAt).FirstAsync();
            return _mapper.Map<BookingPolicyDto>(policy);
        }


        public async Task UpdateAsync(BookingPolicyDto dto)
        {
            var entity = await _db.BookingPolicies.FirstAsync(x => x.Id == dto.Id);
            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}