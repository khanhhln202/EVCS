using EVCS.Services.DTOs;
using EVCS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EVCS.Web.Controllers
{
    [Authorize]
    [Route("Bookings")]
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var bookings = await _bookingService.GetMyBookingsAsync(userId);
                return View(bookings);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể tải danh sách đặt chỗ: " + ex.Message;
                return View(new List<BookingListItemDto>());
            }
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var bookingDetails = await _bookingService.GetBookingDetailsAsync(id);

                if (bookingDetails == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin đặt chỗ.";
                    return RedirectToAction(nameof(Index));
                }

                // Fix: So sánh string thay vì Guid
                if (bookingDetails.DriverId.ToString() != userId)
                {
                    TempData["Error"] = "Bạn không có quyền xem thông tin đặt chỗ này.";
                    return RedirectToAction(nameof(Index));
                }

                return View(bookingDetails);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể tải thông tin chi tiết: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("Cancel/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                if (!Guid.TryParse(userId, out var driverId))
                {
                    TempData["Error"] = "Thông tin người dùng không hợp lệ.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _bookingService.CancelBookingAsync(id, driverId);

                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể hủy đặt chỗ: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("Cancel/{id}/FromDetails")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelFromDetails(Guid id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                if (!Guid.TryParse(userId, out var driverId))
                {
                    TempData["Error"] = "Thông tin người dùng không hợp lệ.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var result = await _bookingService.CancelBookingAsync(id, driverId);

                if (result.Success)
                {
                    TempData["Success"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction(nameof(Details), new { id });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không thể hủy đặt chỗ: " + ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}

