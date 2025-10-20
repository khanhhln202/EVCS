using EVCS.DataAccess.Data;
using EVCS.Models.Entities;
using EVCS.Models.Enums;
using EVCS.Services.Interfaces;
using EVCS.Services.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EVCS.Web.Controllers
{
    public class StationsController : Controller
    {
        private readonly IStationService _svc;
        private readonly IStationQueryService _query;
        private readonly ApplicationDbContext _db;

        public StationsController(IStationService svc, IStationQueryService query, ApplicationDbContext db)
        {
            _svc = svc;
            _query = query;
            _db = db;
        }

        public IActionResult Index() => RedirectToPage("/Stations/Browse");
        public IActionResult Map() => View();
        public IActionResult Browse() => View();
        [HttpGet] public IActionResult List() => View();

        [HttpGet]
        public async Task<IActionResult> MapData([FromQuery] string? city, [FromQuery] string? connectorType, [FromQuery] bool? openNow,
                                                [FromQuery] string? chargerType, [FromQuery] decimal? minPowerKw, [FromQuery] decimal? maxPowerKw,
                                                [FromQuery] bool? online)
        {
            var criteria = new StationSearchCriteria
            {
                City = city,
                ConnectorType = connectorType,
                OpenNow = openNow,
                ChargerType = string.IsNullOrWhiteSpace(chargerType) ? null : chargerType,
                MinPowerKw = minPowerKw,
                MaxPowerKw = maxPowerKw
            };
            var data = await _query.SearchAsync(criteria);
            if (online.HasValue) data = data.Where(s => s.Online == online.Value).ToList();
            return Json(data);
        }

        // GET: available ports (status Available and not booked)
        [HttpGet]
        public async Task<IActionResult> AvailablePorts([FromQuery] Guid chargerId)
        {
            var exists = await _db.ChargerUnits.AsNoTracking().AnyAsync(c => !c.IsDeleted && c.Id == chargerId);
            if (!exists) return NotFound();

            var nowUtc = DateTime.UtcNow;
            var activeBookedPorts = await _db.Bookings.AsNoTracking()
                .Where(b => !b.IsDeleted
                            && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
                            && b.EndAtUtc > nowUtc)
                .Select(b => b.ConnectorPortId)
                .ToListAsync();
            var booked = activeBookedPorts.ToHashSet();

            var ports = await _db.ConnectorPorts.AsNoTracking()
                .Where(p => !p.IsDeleted && p.ChargerId == chargerId && p.Status == ConnectorPortStatus.Available && !booked.Contains(p.Id))
                .OrderBy(p => p.IndexNo)
                .Select(p => new { id = p.Id, indexNo = p.IndexNo, connectorType = p.ConnectorType, maxPowerKw = p.MaxPowerKw, pricePerKwh = p.DefaultPricePerKwh })
                .ToListAsync();

            return Json(ports);
        }
    }
}
