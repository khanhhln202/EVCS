using EVCS.Services.Admin;
using EVCS.Services.DTOs;
using EVCS.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVCS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class StationsController : Controller
    {
        private readonly IStationAdminService _svc;
        private readonly IChargerAdminService _chargerSvc;


        public StationsController(IStationAdminService svc, IChargerAdminService chargerSvc)
        { _svc = svc; _chargerSvc = chargerSvc; }


        public async Task<IActionResult> Index(string? city)
        {
            var list = await _svc.GetAllAsync(city);
            ViewBag.City = city;
            return View(list);
        }


        public IActionResult Create() => View(new StationUpsertDto());


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StationUpsertDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _svc.CreateAsync(dto);
            TempData["success"] = "Created station";
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(Guid id)
        {
            var dto = await _svc.GetAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StationUpsertDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try { await _svc.UpdateAsync(dto); TempData["success"] = "Updated station"; }
            catch (DbUpdateConcurrencyException) { TempData["error"] = "Concurrency conflict. Please reload."; }
            return RedirectToAction(nameof(Index));
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _svc.SoftDeleteAsync(id);
            TempData["success"] = "Deleted station";
            return RedirectToAction(nameof(Index));
        }
    }
}
