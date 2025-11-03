using EVCS.Services.DTOs;
using EVCS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EVCS.Web.Controllers
{
    [Authorize] // Bắt buộc user phải đăng nhập
    [Route("Bookings")] // URL sẽ là /Bookings
    public class BookingsController : Controller // <-- Kế thừa từ Controller (không phải ControllerBase)
    {
        private readonly IBookingService _bookingService;

        // 1. Inject service
        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // 2. Tạo Action "Index" (trang "Đặt chỗ của tôi")
        //    Nó sẽ gọi hàm GetMyBookingsAsync bạn vừa cung cấp
        //    Route: GET /Bookings
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy ID của user đã đăng nhập
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(); // Lỗi nếu không tìm thấy User ID
                }

                // Gọi hàm GetMyBookingsAsync từ service
                var bookings = await _bookingService.GetMyBookingsAsync(userId);

                // Trả về View "Index.cshtml" kèm theo danh sách DTO
                return View(bookings);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, hiển thị thông báo và trả về list rỗng
                TempData["Error"] = "Không thể tải danh sách đặt chỗ: " + ex.Message;
                return View(new List<BookingListItemDto>());
            }
        }
    }
}

