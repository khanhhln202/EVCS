using EVCS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EVCS.Web.Controllers
{
    public class StationsController : Controller
    {
        private readonly IStationService _svc;
        public StationsController(IStationService svc) { _svc = svc; }


        public async Task<IActionResult> Index(string? city)
        {
            var data = await _svc.GetOnlineStationsAsync(city);
            return View(data);
        }
    }
}
