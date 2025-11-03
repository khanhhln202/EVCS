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
        public async Task<List<BookingListItemDto>> GetMyBookingsAsync(string driverId)

        {

            var driverGuid = Guid.Parse(driverId);



            // 1. Query t?t c? booking c?a user này

            var bookings = await _db.Bookings.AsNoTracking()

                .Where(b => b.DriverId == driverGuid && !b.IsDeleted)

                .Include(b => b.ConnectorPort.Charger.Station)

                .OrderByDescending(b => b.CreatedAt) // M?i nh?t lên ð?u

                .ToListAsync();



            var bookingIds = bookings.Select(b => b.Id).ToList();



            // 2. T?m Payment (thanh toán) m?i nh?t cho m?i booking

            var latestPayments = await _db.Payments.AsNoTracking()

                .Where(p => p.BookingId.HasValue && bookingIds.Contains(p.BookingId.Value))

                .GroupBy(p => p.BookingId)

                .Select(g => g.OrderByDescending(p => p.CreatedAt).First()) // L?y cái m?i nh?t

                .ToDictionaryAsync(p => p.BookingId.Value, p => new { p.Id, p.Status });



            // 3. Map sang DTO

            var dtos = bookings.Select(b =>

            {

                // L?y thông tin thanh toán

                latestPayments.TryGetValue(b.Id, out var paymentInfo);



                return new BookingListItemDto

                {

                    Id = b.Id,

                    BookingCode = b.Code,

                    StationName = b.ConnectorPort?.Charger?.Station?.Name ?? "N/A",

                    HoldWindowText = $"{b.StartAtUtc.ToLocalTime():HH:mm} - {b.EndAtUtc.ToLocalTime():HH:mm dd/MM}",

                    BookingStatus = b.Status,

                    Amount = b.DepositAmount,

                    Currency = b.DepositCurrency,

                    // Gán thông tin Payment

                    PaymentId = paymentInfo?.Id ?? Guid.Empty,

                    PaymentStatus = paymentInfo?.Status ?? PaymentStatus.Created // M?c ð?nh là 'Chýa TT'

                };

            }).ToList();



            return dtos;

        }
    }
}