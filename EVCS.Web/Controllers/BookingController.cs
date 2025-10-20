using EVCS.Services.DTOs;
using EVCS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EVCS.Web.Controllers
{
    [ApiController]
    [Route("api/booking")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _svc;
        public BookingController(IBookingService svc) { _svc = svc; }

        // GET /api/booking/available-ports?chargerId=...
        [HttpGet("available-ports")]
        public async Task<IActionResult> AvailablePorts([FromQuery] Guid chargerId)
        {
            if (chargerId == Guid.Empty) return BadRequest(new { message = "Thiếu chargerId." });
            var ports = await _svc.GetAvailablePortsAsync(chargerId);
            return Ok(ports);
        }

        public sealed class BookRequest { public Guid PortId { get; set; } public int? Minutes { get; set; } }

        // POST /api/booking/book
        [Authorize]
        [HttpPost("book")]
        public async Task<IActionResult> Book([FromBody] BookRequest req)
        {
            if (req == null || req.PortId == Guid.Empty) return BadRequest(new { message = "Thiếu thông tin cổng sạc." });
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var driverId)) return Unauthorized();

            try
            {
                var result = await _svc.CreateBookingAsync(req.PortId, driverId, Math.Clamp(req.Minutes ?? 15, 5, 180));
                var checkoutUrl = Url.Page("/Payments/Checkout", new { id = result.PaymentId });
                return Ok(new { ok = true, redirectUrl = checkoutUrl, bookingCode = result.BookingCode });
            }
            catch (InvalidOperationException ex) when (ex.Message == "PORT_NOT_FOUND")
            { return NotFound(new { message = "Không tìm thấy cổng sạc." }); }
            catch (InvalidOperationException ex) when (ex.Message == "PORT_NOT_AVAILABLE" || ex.Message == "PORT_ALREADY_BOOKED")
            { return Conflict(new { message = "Cổng sạc không còn trống hoặc đã được đặt." }); }
        }
    }
}