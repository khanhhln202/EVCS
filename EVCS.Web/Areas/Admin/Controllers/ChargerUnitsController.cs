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
    public class ChargerUnitsController : Controller
    {
        private readonly IChargerAdminService _svc;
        private readonly IStationAdminService _stationSvc;


        public ChargerUnitsController(IChargerAdminService svc, IStationAdminService stationSvc)
        { _svc = svc; _stationSvc = stationSvc; }


        public async Task<IActionResult> Index(Guid stationId)
        {
            ViewBag.StationId = stationId;
            var list = await _svc.GetByStationAsync(stationId);

            // Provide items via ViewBag to match the view
            ViewBag.Items = list;

            // Provide a PagedResult model to satisfy the view's @model
            var model = new EVCS.Web.ViewModels.PagedResult
            {
                Page = 1,
                PageSize = list.Count,
                TotalCount = list.Count,
                BasePath = Request.Path,     // keep clean path; stationId remains in Query
                Query = Request.Query
            };

            return View(model);
        }


        public async Task<IActionResult> Create(Guid stationId)
        {
            ViewBag.StationId = stationId;
            return View(new ChargerUnitUpsertDto { StationId = stationId });
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChargerUnitUpsertDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _svc.CreateAsync(dto);
            TempData["success"] = "Created charger";
            return RedirectToAction(nameof(Index), new { stationId = dto.StationId });
        }


        public async Task<IActionResult> Edit(Guid id)
        {
            var dto = await _svc.GetAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ChargerUnitUpsertDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try { await _svc.UpdateAsync(dto); TempData["success"] = "Updated charger"; }
            catch (DbUpdateConcurrencyException) { TempData["error"] = "Concurrency conflict. Please reload."; }
            return RedirectToAction(nameof(Index), new { stationId = dto.StationId });
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, Guid stationId)
        {
            await _svc.SoftDeleteAsync(id);
            TempData["success"] = "Deleted charger";
            return RedirectToAction(nameof(Index), new { stationId });
        }
    }
}