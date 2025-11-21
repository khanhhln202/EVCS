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

        // GET /api/booking/available-slots?portId=...&from=...&to=...
        [HttpGet("available-slots")]
        public async Task<IActionResult> AvailableSlots(
            [FromQuery] Guid portId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            if (portId == Guid.Empty) return BadRequest(new { message = "Thiếu portId." });
            
            var slots = await _svc.GetAvailableSlotsAsync(portId, from, to);
            return Ok(slots);
        }

        // POST /api/booking/book
        [Authorize]
        [HttpPost("book")]
        public async Task<IActionResult> Book([FromBody] CreateBookingRequestDto req)
        {
            if (req == null || req.PortId == Guid.Empty) 
                return BadRequest(new { message = "Thiếu thông tin cổng sạc." });

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var driverId)) 
                return Unauthorized();

            try
            {
                var result = await _svc.CreateBookingAsync(req, driverId);
                var checkoutUrl = Url.Action("Pay", "Payment", new { bookingId = result.BookingId });
                
                return Ok(new { 
                    ok = true, 
                    redirectUrl = checkoutUrl, 
                    bookingCode = result.BookingCode 
                });
            }
            catch (InvalidOperationException ex) when (ex.Message == "PORT_NOT_FOUND")
            { return NotFound(new { message = "Không tìm thấy cổng sạc." }); }
            catch (InvalidOperationException ex) when (ex.Message == "PORT_NOT_AVAILABLE")
            { return Conflict(new { message = "Cổng sạc không còn trống." }); }
            catch (InvalidOperationException ex) when (ex.Message == "TIME_SLOT_OCCUPIED")
            { return Conflict(new { message = "Khung giờ đã có người đặt." }); }
            catch (InvalidOperationException ex) when (ex.Message == "INVALID_DURATION")
            { return BadRequest(new { message = "Thời lượng không hợp lệ (30-180 phút)." }); }
            catch (InvalidOperationException ex) when (ex.Message == "DURATION_NOT_BLOCK_ALIGNED")
            { return BadRequest(new { message = "Thời lượng phải là bội số của 15 phút." }); }
            catch (InvalidOperationException ex) when (ex.Message == "START_TIME_TOO_SOON")
            { return BadRequest(new { message = "Thời gian bắt đầu phải sau ít nhất 5 phút." }); }
            catch (Exception ex)
            { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}