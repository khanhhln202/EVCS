using EVCS.Services.Interfaces;
using EVCS.Services.Query;
using Microsoft.AspNetCore.Mvc;

namespace EVCS.Web.Controllers
{
    public class StationsController : Controller
    {
        private readonly IStationService _svc;
        private readonly IStationQueryService _query;

        public StationsController(IStationService svc, IStationQueryService query)
        {
            _svc = svc;
            _query = query;
        }

        public async Task<IActionResult> Index(string? city)
        {
            var data = await _svc.GetOnlineStationsAsync(city);
            return View(data);
        }

        public IActionResult Map() => View();

        [HttpGet]
        public async Task<IActionResult> MapData([FromQuery] string? city, [FromQuery] string? connectorType, [FromQuery] bool? openNow)
        {
            var data = await _query.SearchAsync(new StationSearchCriteria { City = city, ConnectorType = connectorType, OpenNow = openNow });
            return Json(data);
        }
    }
}
