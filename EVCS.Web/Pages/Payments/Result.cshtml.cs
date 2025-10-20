using EVCS.DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EVCS.Web.Pages.Payments
{
    [Authorize]
    public class ResultModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public ResultModel(ApplicationDbContext db) { _db = db; }

        public string BookingCode { get; set; } = "";
        public string StationName { get; set; } = "";
        public string ChargerType { get; set; } = "";
        public string PowerKw { get; set; } = "";
        public int PortIndex { get; set; }
        public string HoldWindowText { get; set; } = "";
        public string AmountText { get; set; } = "";

        public async Task<IActionResult> OnGet(Guid id)
        {
            var payment = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null || payment.BookingId == null) return NotFound();

            var booking = await _db.Bookings.AsNoTracking().FirstAsync(b => b.Id == payment.BookingId);
            var port = await _db.ConnectorPorts.AsNoTracking().Include(p => p.Charger).ThenInclude(c => c.Station).FirstAsync(p => p.Id == booking.ConnectorPortId);

            BookingCode = booking.Code;
            StationName = port.Charger.Station.Name;
            ChargerType = string.IsNullOrWhiteSpace(port.Charger.Type) ? "AC" : port.Charger.Type!;
            PowerKw = (port.MaxPowerKw ?? port.Charger.MaxPowerKw ?? 0).ToString("#.##", CultureInfo.InvariantCulture) + "kW";
            PortIndex = port.IndexNo;
            HoldWindowText = $"{booking.StartAtUtc.ToLocalTime():HH:mm} - {booking.EndAtUtc.ToLocalTime():HH:mm}";
            AmountText = string.Format(new CultureInfo("vi-VN"), "{0:C0}", payment.Amount);

            return Page();
        }
    }
}