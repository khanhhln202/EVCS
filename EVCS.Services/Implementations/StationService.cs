using AutoMapper;
using AutoMapper.QueryableExtensions;
using EVCS.DataAccess.Data;
using EVCS.Models.Enums;
using EVCS.Services.DTOs;
using EVCS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EVCS.Services.Implementations
{
    public class StationService : IStationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public StationService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        #region Basic Queries

        public async Task<IReadOnlyList<StationListItemDto>> GetOnlineStationsAsync(string? city = null)
        {
            var q = _db.Stations.AsNoTracking()
                .Where(s => !s.IsDeleted && s.Status == StationStatus.Online);

            if (!string.IsNullOrWhiteSpace(city))
                q = q.Where(s => s.City == city);

            return await q
                .ProjectTo<StationListItemDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        #endregion

        #region Advanced Search

        public async Task<IReadOnlyList<StationMapItemDto>> SearchAsync(StationSearchCriteria criteria)
        {
            var nowLocal = DateTime.Now;

            // Step 1: Get stations
            var q = _db.Stations.AsNoTracking().Where(s => !s.IsDeleted);

            if (!string.IsNullOrWhiteSpace(criteria.City))
                q = q.Where(s => s.City == criteria.City);

            q = q.Where(s => s.Latitude != null && s.Longitude != null
                && s.Latitude >= -90 && s.Latitude <= 90
                && s.Longitude >= -180 && s.Longitude <= 180);

            var stations = await q
                .Select(s => new StationRow(
                    s.Id,
                    s.Code,
                    s.Name,
                    s.City,
                    s.Latitude,
                    s.Longitude,
                    s.Status,
                    s.OpenHour,
                    s.CloseHour
                ))
                .ToListAsync();

            if (stations.Count == 0)
                return Array.Empty<StationMapItemDto>();

            var stationIds = stations.Select(s => s.Id).ToList();

            // Step 2: Get chargers
            var chargers = await _db.ChargerUnits.AsNoTracking()
                .Where(cu => !cu.IsDeleted && stationIds.Contains(cu.StationId))
                .Select(cu => new ChargerRow(cu.Id, cu.StationId, cu.Type, cu.MaxPowerKw))
                .ToListAsync();

            var chargerIds = chargers.Select(c => c.Id).ToList();

            // Step 3: Get ports
            var ports = await _db.ConnectorPorts.AsNoTracking()
                .Where(p => !p.IsDeleted && chargerIds.Contains(p.ChargerId))
                .Select(p => new PortRow(
                    p.Id, p.ChargerId, p.ConnectorType,
                    p.MaxPowerKw, p.DefaultPricePerKwh, p.Status
                ))
                .ToListAsync();

            // Step 4: Get booked ports
            var bookedPortIds = await GetBookedPortIdsAsync(ports.Select(p => p.PortId).ToList());
            var bookedSet = bookedPortIds.ToHashSet();

            // Step 5: Build DTOs
            var portsByCharger = ports.GroupBy(p => p.ChargerId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var chargersByStation = BuildChargerSummaries(chargers, portsByCharger, bookedSet);
            var portsByStation = BuildPortStatuses(ports, chargers);

            // Step 6: Apply filters and build result
            var result = BuildStationMapItems(stations, chargersByStation, portsByStation);
            result = ApplyFilters(result, criteria);

            return result;
        }

        #endregion

        #region Available Ports

        public async Task<IReadOnlyList<AvailablePortDto>> GetAvailablePortsAsync(Guid chargerId)
        {
            var exists = await _db.ChargerUnits.AsNoTracking()
                .AnyAsync(c => !c.IsDeleted && c.Id == chargerId);

            if (!exists)
                return Array.Empty<AvailablePortDto>();

            var bookedPortIds = await GetBookedPortIdsAsync(null);
            var bookedSet = bookedPortIds.ToHashSet();

            var ports = await _db.ConnectorPorts.AsNoTracking()
                .Where(p => !p.IsDeleted
                    && p.ChargerId == chargerId
                    && p.Status == ConnectorPortStatus.Available
                    && !bookedSet.Contains(p.Id))
                .OrderBy(p => p.IndexNo)
                .Select(p => new AvailablePortDto(
                    p.Id,
                    p.IndexNo,
                    p.ConnectorType,
                    p.MaxPowerKw,
                    p.DefaultPricePerKwh
                ))
                .ToListAsync();

            return ports;
        }

        #endregion

        #region Private Helper Methods

        private async Task<List<Guid>> GetBookedPortIdsAsync(List<Guid>? portIdsFilter)
        {
            var nowUtc = DateTime.UtcNow;
            var query = _db.Bookings.AsNoTracking()
                .Where(b => !b.IsDeleted
                    && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
                    && b.EndAtUtc > nowUtc);

            if (portIdsFilter != null && portIdsFilter.Any())
                query = query.Where(b => portIdsFilter.Contains(b.ConnectorPortId));

            return await query
                .Select(b => b.ConnectorPortId)
                .Distinct()
                .ToListAsync();
        }

        private Dictionary<Guid, List<ChargerSummaryDto>> BuildChargerSummaries(
            List<ChargerRow> chargers,
            Dictionary<Guid, List<PortRow>> portsByCharger,
            HashSet<Guid> bookedSet)
        {
            return chargers
                .GroupBy(c => c.StationId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(cu =>
                    {
                        portsByCharger.TryGetValue(cu.Id, out var pList);
                        pList ??= new List<PortRow>();

                        var available = pList.Count(p =>
                            p.Status == ConnectorPortStatus.Available
                            && !bookedSet.Contains(p.PortId));
                        var total = pList.Count;

                        decimal? maxPowerKw = cu.MaxPowerKw ??
                            pList.Where(p => p.MaxPowerKw != null)
                                .Select(p => (decimal?)p.MaxPowerKw)
                                .DefaultIfEmpty()
                                .Max();

                        decimal? pricePerKwh = pList
                            .Select(p => (decimal?)p.DefaultPricePerKwh)
                            .DefaultIfEmpty()
                            .Min();

                        var inferredType = InferChargerType(
                            pList.Select(p => p.ConnectorType),
                            cu.Type);

                        return new ChargerSummaryDto(
                            Id: cu.Id,
                            Type: inferredType,
                            MaxPowerKw: maxPowerKw,
                            Available: available,
                            Total: total,
                            PricePerKwh: pricePerKwh
                        );
                    })
                    .OrderBy(c => c.Type)
                    .ThenByDescending(c => c.MaxPowerKw)
                    .ToList()
                );
        }

        private Dictionary<Guid, List<PortStatusDto>> BuildPortStatuses(
            List<PortRow> ports,
            List<ChargerRow> chargers)
        {
            var chargerStationLookup = chargers.ToDictionary(c => c.Id, c => c.StationId);

            return ports
                .GroupBy(p => chargerStationLookup[p.ChargerId])
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(p => new PortStatusDto(
                        p.ConnectorType,
                        p.Status.ToString(),
                        p.DefaultPricePerKwh
                    )).ToList()
                );
        }

        private List<StationMapItemDto> BuildStationMapItems(
            List<StationRow> stations,
            Dictionary<Guid, List<ChargerSummaryDto>> chargersByStation,
            Dictionary<Guid, List<PortStatusDto>> portsByStation)
        {
            var nowLocal = DateTime.Now;
            var result = new List<StationMapItemDto>(stations.Count);

            foreach (var s in stations)
            {
                var openNow = (s.OpenHour != null && s.CloseHour != null)
                    ? (TimeOnly.FromDateTime(nowLocal) >= s.OpenHour
                        && TimeOnly.FromDateTime(nowLocal) <= s.CloseHour)
                    : (bool?)null;

                string? hoursText = null;
                if (s.OpenHour != null && s.CloseHour != null)
                {
                    var closeText = s.CloseHour.Value == TimeOnly.MinValue
                        ? "24:00"
                        : s.CloseHour.Value.ToString("HH\\:mm");
                    hoursText = $"{s.OpenHour:HH\\:mm} - {closeText}";
                }

                portsByStation.TryGetValue(s.Id, out var portDtos);
                chargersByStation.TryGetValue(s.Id, out var chargerDtos);

                var dto = new StationMapItemDto(
                    s.Id, s.Code, s.Name, s.City,
                    s.Latitude, s.Longitude,
                    s.Status == StationStatus.Online,
                    openNow,
                    portDtos ?? new List<PortStatusDto>()
                )
                {
                    Hours = hoursText,
                    Chargers = chargerDtos ?? new List<ChargerSummaryDto>()
                };

                result.Add(dto);
            }

            return result;
        }

        private List<StationMapItemDto> ApplyFilters(
            List<StationMapItemDto> result,
            StationSearchCriteria criteria)
        {
            // Legacy connector type filter
            if (!string.IsNullOrWhiteSpace(criteria.ConnectorType))
            {
                result = result.Where(x => x.Ports.Any(p =>
                    string.Equals(p.ConnectorType ?? string.Empty,
                        criteria.ConnectorType,
                        StringComparison.OrdinalIgnoreCase)
                )).ToList();
            }

            // Charger type filter
            if (!string.IsNullOrWhiteSpace(criteria.ChargerType))
            {
                result = result.Where(x => x.Chargers != null && x.Chargers.Any(c =>
                    string.Equals(c.Type ?? string.Empty,
                        criteria.ChargerType,
                        StringComparison.OrdinalIgnoreCase)
                )).ToList();
            }

            // Power range filter
            if (criteria.MinPowerKw.HasValue || criteria.MaxPowerKw.HasValue)
            {
                decimal min = criteria.MinPowerKw ?? decimal.Zero;
                decimal max = criteria.MaxPowerKw ?? decimal.MaxValue;
                if (min > max) (min, max) = (max, min);

                result = result.Where(x => x.Chargers != null && x.Chargers.Any(c =>
                    c.MaxPowerKw.HasValue
                    && c.MaxPowerKw.Value >= min
                    && c.MaxPowerKw.Value <= max
                )).ToList();
            }

            // Open now filter
            if (criteria.OpenNow.HasValue)
            {
                result = result.Where(x => x.OpenNow == criteria.OpenNow.Value).ToList();
            }

            return result;
        }

        private static string InferChargerType(IEnumerable<string?> connectorTypes, string? fallback)
        {
            if (!string.IsNullOrWhiteSpace(fallback))
                return fallback!;

            var types = connectorTypes
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t!.ToUpperInvariant())
                .ToList();

            if (types.Any(t => t.Contains("GB/T") || t.Contains("GBT")))
                return "GB/T";
            if (types.Any(t => t.Contains("CCS") || t.Contains("CHADEMO")))
                return "DC";

            return "AC";
        }

        #endregion

        #region Helper Record Types

        private sealed record StationRow(
            Guid Id,
            string Code,
            string Name,
            string? City,
            double? Latitude,
            double? Longitude,
            StationStatus Status,
            TimeOnly? OpenHour,
            TimeOnly? CloseHour
        );

        private sealed record ChargerRow(
            Guid Id,
            Guid StationId,
            string? Type,
            decimal? MaxPowerKw
        );

        private sealed record PortRow(
            Guid PortId,
            Guid ChargerId,
            string? ConnectorType,
            decimal? MaxPowerKw,
            decimal DefaultPricePerKwh,
            ConnectorPortStatus Status
        );

        #endregion
    }
}