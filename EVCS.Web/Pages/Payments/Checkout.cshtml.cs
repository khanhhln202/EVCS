using EVCS.DataAccess.Data;
using EVCS.Models.Entities;
using EVCS.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EVCS.Web.Pages.Payments
{
    [Authorize]
    public class CheckoutModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public CheckoutModel(ApplicationDbContext db) { _db = db; }

        [BindProperty] public Guid PaymentId { get; set; }
        public string? Error { get; set; }

        public string BookingCode { get; set; } = "";
        public string StationName { get; set; } = "";
        public string ChargerType { get; set; } = "";
        public string PowerKw { get; set; } = "";
        public int PortIndex { get; set; }
        public string HoldWindowText { get; set; } = "";
        public string AmountText { get; set; } = "";

        public async Task<IActionResult> OnGet(Guid id)
        {
            var vm = await LoadAsync(id);
            if (!vm.ok) { Error = vm.error; return Page(); }
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var vm = await LoadAsync(PaymentId);
            if (!vm.ok) { Error = vm.error; return Page(); }

            // Mark paid and confirm booking
            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var payment = await _db.Payments.FirstAsync(p => p.Id == PaymentId);
                if (payment.Status == PaymentStatus.Paid)
                {
                    return RedirectToPage("/Payments/Result", new { id = payment.Id });
                }
                var booking = await _db.Bookings.FirstAsync(b => b.Id == payment.BookingId);

                payment.Status = PaymentStatus.Paid;
                payment.PaidAtUtc = DateTime.UtcNow;
                payment.UpdatedAt = DateTime.UtcNow;

                booking.Status = BookingStatus.Confirmed;
                booking.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return RedirectToPage("/Payments/Result", new { id = payment.Id });
            }
            catch
            {
                await tx.RollbackAsync();
                Error = "Thanh toán thất bại, vui lòng thử lại.";
                return Page();
            }
        }

        private async Task<(bool ok, string? error)> LoadAsync(Guid paymentId)
        {
            PaymentId = paymentId;

            var payment = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null) return (false, "Không tìm thấy giao dịch.");
            if (payment.BookingId == null) return (false, "Giao dịch không hợp lệ.");

            var booking = await _db.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == payment.BookingId && !b.IsDeleted);
            if (booking == null) return (false, "Không tìm thấy đặt chỗ.");
            if (booking.EndAtUtc <= DateTime.UtcNow && booking.Status != BookingStatus.Confirmed)
                return (false, "Đặt chỗ đã hết hạn.");

            var port = await _db.ConnectorPorts.AsNoTracking()
                .Include(p => p.Charger)
                .ThenInclude(c => c.Station)
                .FirstOrDefaultAsync(p => p.Id == booking.ConnectorPortId);
            if (port == null) return (false, "Cổng sạc không tồn tại.");

            BookingCode = booking.Code;
            StationName = port.Charger.Station.Name;
            ChargerType = string.IsNullOrWhiteSpace(port.Charger.Type) ? "AC" : port.Charger.Type!;
            PowerKw = (port.MaxPowerKw ?? port.Charger.MaxPowerKw ?? 0).ToString("#.##", CultureInfo.InvariantCulture) + "kW";
            PortIndex = port.IndexNo;
            HoldWindowText = $"{booking.StartAtUtc.ToLocalTime():HH:mm} - {booking.EndAtUtc.ToLocalTime():HH:mm}";
            AmountText = string.Format(new CultureInfo("vi-VN"), "{0:C0}", payment.Amount);

            return (true, null);
        }
    }
}