using EVCS.DataAccess.Data;
using EVCS.Models.Enums;
using EVCS.Services.DTOs;
using EVCS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EVCS.Web.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ApplicationDbContext _db;

        public PaymentController(IPaymentService paymentService, ApplicationDbContext db)
        {
            _paymentService = paymentService;
            _db = db;
        }

        [HttpGet("Pay/{bookingId}")]
        public async Task<IActionResult> Pay(Guid bookingId, CancellationToken cancellationToken)
        {
            if (bookingId == Guid.Empty)
                return BadRequest("Booking ID không hợp lệ.");

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Không xác định được người dùng.");

                if (!Guid.TryParse(userId, out var driverId))
                    return Unauthorized("User ID không hợp lệ.");

                // Load thông tin booking đầy đủ
                var booking = await _db.Bookings
                    .AsNoTracking()
                    .Include(b => b.ConnectorPort)
                        .ThenInclude(p => p.Charger)
                        .ThenInclude(c => c.Station)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.DriverId == driverId && !b.IsDeleted, cancellationToken);

                if (booking == null)
                    return NotFound("Không tìm thấy đặt chỗ hoặc bạn không có quyền truy cập.");

                // Kiểm tra trạng thái booking
                if (booking.Status != BookingStatus.Pending)
                {
                    TempData["Error"] = "Booking này không cần thanh toán hoặc đã được thanh toán.";
                    return RedirectToAction("Index", "Bookings");
                }

                // Gọi service để chuẩn bị thanh toán Stripe
                var dto = await _paymentService.PreparePaymentAsync(bookingId, userId, cancellationToken);
                if (dto == null)
                    return NotFound("Không tạo được thông tin thanh toán.");

                // Tính toán thời lượng
                var durationMinutes = (int)(booking.EndAtUtc - booking.StartAtUtc).TotalMinutes;

                // Tạo ViewModel cho View
                var model = new PaymentViewModel
                {
                    PublishableKey = dto.PublishableKey,
                    ClientSecret = dto.ClientSecret,
                    PaymentId = dto.PaymentId,
                    BookingId = booking.Id,
                    TotalAmount = booking.DepositAmount,
                    Currency = booking.DepositCurrency,
                    BookingCode = booking.Code,
                    BookingType = booking.Type,
                    StationName = booking.ConnectorPort?.Charger?.Station?.Name ?? "N/A",
                    StationCity = booking.ConnectorPort?.Charger?.Station?.City,
                    ChargerType = booking.ConnectorPort?.Charger?.Type,
                    PortName = $"Cổng #{booking.ConnectorPort?.IndexNo} ({booking.ConnectorPort?.ConnectorType})",
                    StartTime = booking.StartAtUtc.ToLocalTime(),
                    EndTime = booking.EndAtUtc.ToLocalTime(),
                    DurationMinutes = durationMinutes
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Payment Error] {ex.Message}");
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Index", "Bookings");
            }
        }

        [HttpGet("Complete")]
        public async Task<IActionResult> Complete(Guid payment_id, string redirect_status, CancellationToken cancellationToken)
        {
            if (payment_id == Guid.Empty)
                return BadRequest("Payment ID không hợp lệ.");

            var payment = await _db.Payments
                .AsNoTracking()
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.Id == payment_id, cancellationToken);

            ViewBag.PaymentId = payment_id;
            ViewBag.BookingCode = payment?.Booking?.Code ?? "N/A";

            if (redirect_status == "succeeded")
            {
                ViewBag.Title = "Thanh toán thành công";
                ViewBag.Message = "Thanh toán của bạn đã thành công. Đặt chỗ đã được xác nhận!";
                ViewBag.IsSuccess = true;
            }
            else
            {
                ViewBag.Title = "Thanh toán chưa hoàn tất";
                ViewBag.Message = $"Trạng thái thanh toán: {redirect_status}. Vui lòng thử lại.";
                ViewBag.IsSuccess = false;
            }

            return View();
        }
    }
}