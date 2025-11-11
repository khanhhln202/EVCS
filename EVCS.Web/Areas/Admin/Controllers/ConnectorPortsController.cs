using EVCS.Services.DTOs;
using EVCS.Services.Interfaces.Admin;
using EVCS.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVCS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class ConnectorPortsController : Controller
    {
        private readonly IConnectorPortAdminService _svc;

        public ConnectorPortsController(IConnectorPortAdminService svc) { _svc = svc; }

        public async Task<IActionResult> Index(Guid chargerId)
        {
            ViewBag.ChargerId = chargerId;
            var list = await _svc.GetByChargerAsync(chargerId);

            ViewBag.Items = list; // match the view

            var model = new EVCS.Web.ViewModels.PagedResult
            {
                Page = 1,
                PageSize = list.Count,
                TotalCount = list.Count,
                BasePath = Request.Path,
                Query = Request.Query
            };

            return View(model);
        }

        public IActionResult Create(Guid chargerId)
            => View(new ConnectorPortUpsertDto { ChargerId = chargerId, IndexNo = 1, DefaultPricePerKwh = 3000 });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConnectorPortUpsertDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _svc.CreateAsync(dto);
            TempData["success"] = "Created port";
            return RedirectToAction(nameof(Index), new { chargerId = dto.ChargerId });
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var dto = await _svc.GetAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ConnectorPortUpsertDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            try { await _svc.UpdateAsync(dto); TempData["success"] = "Updated port"; }
            catch (DbUpdateConcurrencyException) { TempData["error"] = "Concurrency conflict. Please reload."; }
            return RedirectToAction(nameof(Index), new { chargerId = dto.ChargerId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, Guid chargerId)
        {
            await _svc.SoftDeleteAsync(id);
            TempData["success"] = "Deleted port";
            return RedirectToAction(nameof(Index), new { chargerId });
        }
    }
}
