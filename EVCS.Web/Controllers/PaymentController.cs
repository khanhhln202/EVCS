using EVCS.DataAccess.Data;
using EVCS.Services.DTOs;
using EVCS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EVCS.Web.Controllers
{
    [Route("[controller]")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ApplicationDbContext _db; 
        public PaymentController(IPaymentService paymentService,ApplicationDbContext db)
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

                // Gọi service để chuẩn bị thanh toán
                var dto = await _paymentService.PreparePaymentAsync(bookingId, userId, cancellationToken);
                if (dto == null)
                    return NotFound("Không tạo được thông tin thanh toán.");

                // Tạo ViewModel cho View hiển thị
                var model = new PaymentViewModel
                {
                    PublishableKey = dto.PublishableKey,
                    ClientSecret = dto.ClientSecret,
                    PaymentId = dto.PaymentId,
                    TotalAmount = dto.Amount / 100m, // vì Stripe trả về số tiền theo cents
                    BookingCode = $"#{bookingId.ToString().Substring(0, 8)}",
                    StationName = "EVCS Station",
                    PortName = "Charger 01",
                    Minutes = 60
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Payment Error] {ex.Message}");
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Details", "Booking", new { id = bookingId });
            }
        }

        [HttpGet("Complete")]
        public IActionResult Complete(Guid payment_id, string redirect_status)
        {   
            if (redirect_status == "succeeded")
            {
                ViewBag.Title = "Thanh toán thành công";
                ViewBag.Message = "Thanh toán của bạn đã thành công. Chúng tôi đang xác nhận đặt chỗ.";
            }
            else
            {
                ViewBag.Title = "Thanh toán chưa hoàn tất";
                ViewBag.Message = $"Trạng thái thanh toán: {redirect_status}. Vui lòng thử lại.";
            }
            ViewBag.PaymentId = payment_id;
            return View();
        }
    }
}
