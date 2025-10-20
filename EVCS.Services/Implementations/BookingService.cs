using EVCS.DataAccess.Data;
using EVCS.Models.Entities;
using EVCS.Models.Enums;
using EVCS.Services.DTOs;
using EVCS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EVCS.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _db;
        public BookingService(ApplicationDbContext db) { _db = db; }

        public async Task<IReadOnlyList<AvailablePortDto>> GetAvailablePortsAsync(Guid chargerId)
        {
            var exists = await _db.ChargerUnits.AsNoTracking().AnyAsync(c => !c.IsDeleted && c.Id == chargerId);
            if (!exists) return Array.Empty<AvailablePortDto>();

            var nowUtc = DateTime.UtcNow;
            var activeBooked = await _db.Bookings.AsNoTracking()
                .Where(b => !b.IsDeleted
                            && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
                            && b.EndAtUtc > nowUtc)
                .Select(b => b.ConnectorPortId)
                .ToListAsync();
            var bookedSet = activeBooked.ToHashSet();

            var ports = await _db.ConnectorPorts.AsNoTracking()
                .Where(p => !p.IsDeleted
                            && p.ChargerId == chargerId
                            && p.Status == ConnectorPortStatus.Available
                            && !bookedSet.Contains(p.Id))
                .OrderBy(p => p.IndexNo)
                .Select(p => new AvailablePortDto(
                    p.Id, p.IndexNo, p.ConnectorType, p.MaxPowerKw, p.DefaultPricePerKwh
                ))
                .ToListAsync();

            return ports;
        }

        public async Task<BookResult> CreateBookingAsync(Guid portId, Guid driverId, int holdMinutes)
        {
            var port = await _db.ConnectorPorts
                .Include(p => p.Charger)
                .ThenInclude(c => c.Station)
                .FirstOrDefaultAsync(p => p.Id == portId && !p.IsDeleted);
            if (port == null) throw new InvalidOperationException("PORT_NOT_FOUND");
            if (port.Status != ConnectorPortStatus.Available) throw new InvalidOperationException("PORT_NOT_AVAILABLE");

            var nowUtc = DateTime.UtcNow;
            var alreadyBooked = await _db.Bookings.AnyAsync(b =>
                !b.IsDeleted && b.ConnectorPortId == port.Id
                && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
                && b.EndAtUtc > nowUtc);
            if (alreadyBooked) throw new InvalidOperationException("PORT_ALREADY_BOOKED");

            var policy = await _db.BookingPolicies.AsNoTracking().OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();
            var isDc = string.Equals(port.Charger.Type, "DC", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(port.Charger.Type, "GB/T", StringComparison.OrdinalIgnoreCase);
            var deposit = policy != null ? (isDc ? policy.DcDeposit : policy.AcDeposit) : (isDc ? 50000m : 30000m);
            var hold = policy?.HoldMinutes ?? Math.Clamp(holdMinutes, 5, 180);

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                Code = $"BK-{DateTime.UtcNow:yyyyMMddHHmmss}-{port.IndexNo:D2}",
                DriverId = driverId,
                ConnectorPortId = port.Id,
                StartAtUtc = nowUtc,
                EndAtUtc = nowUtc.AddMinutes(hold),
                Status = BookingStatus.Pending,
                DepositAmount = deposit,
                DepositCurrency = "VND"
            };
            _db.Bookings.Add(booking);

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                Provider = default,              // set to your real provider later
                Kind = PaymentKind.Deposit,
                Amount = deposit,
                Currency = "VND",
                Status = PaymentStatus.Created
            };
            _db.Payments.Add(payment);

            await _db.SaveChangesAsync();
            return new BookResult(booking.Id, booking.Code, payment.Id, deposit, "VND");
        }
    }
}