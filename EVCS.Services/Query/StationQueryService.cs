using AutoMapper;
using EVCS.DataAccess.Data;
using EVCS.Models.Enums;
using EVCS.Services.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.Query
{
    public class StationQueryService : IStationQueryService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public StationQueryService(ApplicationDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }


        public async Task<IReadOnlyList<StationMapItemDto>> SearchAsync(StationSearchCriteria criteria)
        {
            var nowLocal = DateTime.Now; // UI địa phương; có thể dùng TimezoneId để chính xác hơn
            var q = _db.Stations.AsNoTracking().Where(s => !s.IsDeleted && s.Status == StationStatus.Online);
            if (!string.IsNullOrWhiteSpace(criteria.City)) q = q.Where(s => s.City == criteria.City);


            var list = await q.Select(s => new StationMapItemDto(
            s.Id,
            s.Code,
            s.Name,
            s.City,
            s.Latitude,
            s.Longitude,
            s.Status == StationStatus.Online,
            s.OpenHour != null && s.CloseHour != null ?
            (TimeOnly.FromDateTime(nowLocal) >= s.OpenHour && TimeOnly.FromDateTime(nowLocal) <= s.CloseHour) : null,
            _db.ConnectorPorts
            .Where(p => !p.IsDeleted && _db.ChargerUnits.Any(c => c.Id == p.ChargerId && c.StationId == s.Id && !c.IsDeleted))
            .Select(p => new PortStatusDto(p.ConnectorType, p.Status.ToString(), p.DefaultPricePerKwh))
            .ToList()
            )).ToListAsync();


            if (!string.IsNullOrWhiteSpace(criteria.ConnectorType))
                list = list.Where(x => x.Ports.Any(p => (p.ConnectorType ?? "").Equals(criteria.ConnectorType, StringComparison.OrdinalIgnoreCase))).ToList();
            if (criteria.OpenNow == true)
                list = list.Where(x => x.OpenNow == true).ToList();


            return list;
        }
    }
}
