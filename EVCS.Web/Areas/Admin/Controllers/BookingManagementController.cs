using EVCS.Services.DTOs;
using EVCS.Services.Interfaces.Admin;
using EVCS.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace EVCS.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.RoleAdmin},{SD.RoleStaff}")]
    public class BookingManagementController : Controller
    {
        private readonly IBookingManagementService _service;

        public BookingManagementController(IBookingManagementService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] BookingFilterDto filter)
        {
            var result = await _service.GetBookingsAsync(filter);
            ViewBag.Filter = filter;
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var booking = await _service.GetBookingDetailAsync(id);
                return View(booking);
            }
            catch (InvalidOperationException ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin)]
        public async Task<IActionResult> Cancel(Guid id, string reason)
        {
            try
            {
                await _service.CancelBookingAsync(id, reason);
                TempData["success"] = "Đã hủy booking thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Complete(Guid id)
        {
            try
            {
                await _service.CompleteBookingAsync(id);
                TempData["success"] = "Đã hoàn thành booking.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Stats()
        {
            var stats = await _service.GetStatsAsync();
            return Json(stats);
        }
    }
}