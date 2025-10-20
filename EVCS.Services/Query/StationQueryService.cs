using AutoMapper;
using EVCS.DataAccess.Data;
using EVCS.Models.Enums;
using EVCS.Services.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EVCS.Services.Query
{
    public class StationQueryService : IStationQueryService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public StationQueryService(ApplicationDbContext db, IMapper mapper) { _db = db; _mapper = mapper; }

        // Added PortId to compute availability excluding booked ports
        private sealed record PortRow(Guid PortId, Guid ChargerId, string? ConnectorType, decimal? MaxPowerKw, decimal DefaultPricePerKwh, ConnectorPortStatus Status);

        public async Task<IReadOnlyList<StationMapItemDto>> SearchAsync(StationSearchCriteria criteria)
        {
            var nowLocal = DateTime.Now;

            var q = _db.Stations.AsNoTracking().Where(s => !s.IsDeleted);
            if (!string.IsNullOrWhiteSpace(criteria.City)) q = q.Where(s => s.City == criteria.City);
            q = q.Where(s => s.Latitude != null && s.Longitude != null && s.Latitude >= -90 && s.Latitude <= 90 && s.Longitude >= -180 && s.Longitude <= 180);

            var stations = await q.Select(s => new { s.Id, s.Code, s.Name, s.City, s.Latitude, s.Longitude, s.Status, s.OpenHour, s.CloseHour }).ToListAsync();
            if (stations.Count == 0) return Array.Empty<StationMapItemDto>();

            var stationIds = stations.Select(s => s.Id).ToList();

            var chargers = await _db.ChargerUnits.AsNoTracking()
                .Where(cu => !cu.IsDeleted && stationIds.Contains(cu.StationId))
                .Select(cu => new { cu.Id, cu.StationId, cu.Type, cu.MaxPowerKw })
                .ToListAsync();

            var chargerIds = chargers.Select(c => c.Id).ToList();

            var ports = await _db.ConnectorPorts.AsNoTracking()
                .Where(p => !p.IsDeleted && chargerIds.Contains(p.ChargerId))
                .Select(p => new PortRow(p.Id, p.ChargerId, p.ConnectorType, p.MaxPowerKw, p.DefaultPricePerKwh, p.Status))
                .ToListAsync();

            // Active bookings (Pending/Confirmed, not deleted, not expired)
            var nowUtc = DateTime.UtcNow;
            var bookedPortIds = await _db.Bookings.AsNoTracking()
                .Where(b => !b.IsDeleted
                            && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
                            && b.EndAtUtc > nowUtc
                            && ports.Select(p => p.PortId).Contains(b.ConnectorPortId))
                .Select(b => b.ConnectorPortId)
                .Distinct()
                .ToListAsync();
            var bookedSet = bookedPortIds.ToHashSet();

            var portsByCharger = ports.GroupBy(p => p.ChargerId).ToDictionary(g => g.Key, g => g.ToList());

            static string InferChargerType(IEnumerable<string?> connectorTypes, string? fallback)
            {
                if (!string.IsNullOrWhiteSpace(fallback)) return fallback!;
                var types = connectorTypes.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t!.ToUpperInvariant()).ToList();
                if (types.Any(t => t.Contains("GB/T") || t.Contains("GBT"))) return "GB/T";
                if (types.Any(t => t.Contains("CCS") || t.Contains("CHADEMO"))) return "DC";
                return "AC";
            }

            var chargersByStation = chargers
                .GroupBy(c => c.StationId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(cu =>
                    {
                        portsByCharger.TryGetValue(cu.Id, out var pList);
                        pList ??= new List<PortRow>();

                        // Available = Available status AND not booked
                        var available = pList.Count(p => p.Status == ConnectorPortStatus.Available && !bookedSet.Contains(p.PortId));
                        var total = pList.Count;

                        decimal? maxPowerKw = cu.MaxPowerKw ?? (pList.Where(p => p.MaxPowerKw != null).Select(p => (decimal?)p.MaxPowerKw).DefaultIfEmpty().Max());
                        decimal? pricePerKwh = pList.Select(p => (decimal?)p.DefaultPricePerKwh).DefaultIfEmpty().Min();
                        var inferredType = InferChargerType(pList.Select(p => (string?)p.ConnectorType), cu.Type);

                        return new ChargerSummaryDto(
                            Id: cu.Id,
                            Type: inferredType,
                            MaxPowerKw: maxPowerKw,
                            Available: available,
                            Total: total,
                            PricePerKwh: pricePerKwh
                        );
                    })
                    .OrderBy(c => c.Type).ThenByDescending(c => c.MaxPowerKw).ToList()
                );

            var chargerStationLookup = chargers.ToDictionary(c => c.Id, c => c.StationId);
            var portsByStation = ports.GroupBy(p => chargerStationLookup[p.ChargerId]).ToDictionary(
                g => g.Key,
                g => g.Select(p => new PortStatusDto(p.ConnectorType, p.Status.ToString(), p.DefaultPricePerKwh)).ToList()
            );

            var result = new List<StationMapItemDto>(stations.Count);
            foreach (var s in stations)
            {
                var openNow = (s.OpenHour != null && s.CloseHour != null)
                    ? (TimeOnly.FromDateTime(nowLocal) >= s.OpenHour && TimeOnly.FromDateTime(nowLocal) <= s.CloseHour)
                    : (bool?)null;

                string? hoursText = null;
                if (s.OpenHour != null && s.CloseHour != null)
                {
                    var closeText = s.CloseHour.Value == TimeOnly.MinValue ? "24:00" : s.CloseHour.Value.ToString("HH\\:mm");
                    hoursText = $"{s.OpenHour:HH\\:mm} - {closeText}";
                }

                portsByStation.TryGetValue(s.Id, out var portDtos);
                chargersByStation.TryGetValue(s.Id, out var chargerDtos);

                var dto = new StationMapItemDto(s.Id, s.Code, s.Name, s.City, s.Latitude, s.Longitude, s.Status == StationStatus.Online, openNow, portDtos ?? new List<PortStatusDto>())
                {
                    Hours = hoursText,
                    Chargers = chargerDtos ?? new List<ChargerSummaryDto>()
                };

                result.Add(dto);
            }

            // Legacy connectorType filter
            if (!string.IsNullOrWhiteSpace(criteria.ConnectorType))
            {
                result = result.Where(x => x.Ports.Any(p =>
                    string.Equals(p.ConnectorType ?? string.Empty, criteria.ConnectorType, StringComparison.OrdinalIgnoreCase)
                )).ToList();
            }

            // Charger Type filter
            if (!string.IsNullOrWhiteSpace(criteria.ChargerType))
            {
                result = result.Where(x => x.Chargers != null && x.Chargers.Any(c =>
                    string.Equals(c.Type ?? string.Empty, criteria.ChargerType, StringComparison.OrdinalIgnoreCase)
                )).ToList();
            }

            // Power range filter
            if (criteria.MinPowerKw.HasValue || criteria.MaxPowerKw.HasValue)
            {
                decimal min = criteria.MinPowerKw ?? decimal.Zero;
                decimal max = criteria.MaxPowerKw ?? decimal.MaxValue;
                if (min > max) (min, max) = (max, min);

                result = result.Where(x => x.Chargers != null && x.Chargers.Any(c =>
                    c.MaxPowerKw.HasValue && c.MaxPowerKw.Value >= min && c.MaxPowerKw.Value <= max
                )).ToList();
            }

            // Keep OpenNow (operating hours) filter if used somewhere else
            if (criteria.OpenNow.HasValue)
                result = result.Where(x => x.OpenNow == criteria.OpenNow.Value).ToList();

            return result;
        }
    }
}
