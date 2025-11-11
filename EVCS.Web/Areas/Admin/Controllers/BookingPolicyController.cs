using EVCS.Services.DTOs;
using EVCS.Services.Interfaces.Admin;
using EVCS.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVCS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.RoleAdmin)]
    public class BookingPolicyController : Controller
    {
        private readonly IBookingPolicyService _svc;
        public BookingPolicyController(IBookingPolicyService svc) { _svc = svc; }


        public async Task<IActionResult> Index()
        {
            var dto = await _svc.GetCurrentAsync();
            return View(dto);
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(BookingPolicyDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            await _svc.UpdateAsync(dto);
            TempData["success"] = "Updated booking policy";
            return RedirectToAction(nameof(Index));
        }
    }
}
