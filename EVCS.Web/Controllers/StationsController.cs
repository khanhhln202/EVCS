using EVCS.Services.DTOs;
using EVCS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EVCS.Web.Controllers
{
    public class StationsController : Controller
    {
        private readonly IStationService _stationService;

        public StationsController(IStationService stationService)
        {
            _stationService = stationService;
        }

        #region Views

        public IActionResult Index() => RedirectToPage("/Stations/Browse");

        public IActionResult Map() => View();

        public IActionResult Browse() => View();

        [HttpGet]
        public IActionResult List() => View();

        #endregion

        #region API Endpoints

        /// <summary>
        /// Get map data with filters for stations
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MapData(
            [FromQuery] string? city,
            [FromQuery] string? connectorType,
            [FromQuery] bool? openNow,
            [FromQuery] string? chargerType,
            [FromQuery] decimal? minPowerKw,
            [FromQuery] decimal? maxPowerKw,
            [FromQuery] bool? online)
        {
            var criteria = new Services.DTOs.StationSearchCriteria
            {
                City = city,
                ConnectorType = connectorType,
                OpenNow = openNow,
                ChargerType = string.IsNullOrWhiteSpace(chargerType) ? null : chargerType,
                MinPowerKw = minPowerKw,
                MaxPowerKw = maxPowerKw
            };

            var data = await _stationService.SearchAsync(criteria);

            // Additional online filter
            if (online.HasValue)
            {
                data = data.Where(s => s.Online == online.Value).ToList();
            }

            return Json(data);
        }

        /// <summary>
        /// Get available ports for a specific charger
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AvailablePorts([FromQuery] Guid chargerId)
        {
            if (chargerId == Guid.Empty)
            {
                return BadRequest(new { error = "Invalid charger ID" });
            }

            var ports = await _stationService.GetAvailablePortsAsync(chargerId);

            if (!ports.Any())
            {
                return NotFound(new { error = "No available ports found" });
            }

            return Json(ports);
        }

        #endregion
    }
}