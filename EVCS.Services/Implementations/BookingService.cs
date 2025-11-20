using EVCS.DataAccess.Data;
using EVCS.Models.Entities;
using EVCS.Models.Enums;
using EVCS.Services.DTOs;
using EVCS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EVCS.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _db;

        public BookingService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<PortAvailabilityDto>> GetAvailablePortsAsync(Guid chargerId)
        {
            var charger = await _db.ChargerUnits
                .AsNoTracking()
                .Include(c => c.Ports)
                .FirstOrDefaultAsync(c => c.Id == chargerId && !c.IsDeleted);

            if (charger == null) return Array.Empty<PortAvailabilityDto>();

            var now = DateTime.UtcNow;
            var ports = charger.Ports.Where(p => !p.IsDeleted).ToList();

            var result = new List<PortAvailabilityDto>();

            foreach (var port in ports)
            {
                // Check if port has active booking or session
                var hasActiveBooking = await _db.Bookings.AnyAsync(b =>
                    b.ConnectorPortId == port.Id &&
                    !b.IsDeleted &&
                    b.Status == BookingStatus.Confirmed &&
                    b.StartAtUtc <= now &&
                    b.EndAtUtc >= now);

                var hasActiveSession = await _db.ChargingSessions.AnyAsync(s =>
                    s.ConnectorPortId == port.Id &&
                    !s.IsDeleted &&
                    s.Status == SessionStatus.Started);

                if (!hasActiveBooking && !hasActiveSession && port.Status == ConnectorPortStatus.Available)
                {
                    result.Add(new PortAvailabilityDto
                    {
                        Id = port.Id,
                        IndexNo = port.IndexNo,
                        ConnectorType = port.ConnectorType,
                        PricePerKwh = port.DefaultPricePerKwh
                    });
                }
            }

            return result;
        }

        public async Task<BookResult> CreateBookingAsync(CreateBookingRequestDto request, Guid driverId)
        {
            var policy = await _db.BookingPolicies.FirstOrDefaultAsync()
                ?? throw new InvalidOperationException("POLICY_NOT_FOUND");

            var port = await _db.ConnectorPorts
                .Include(p => p.Charger)
                .FirstOrDefaultAsync(p => p.Id == request.PortId && !p.IsDeleted)
                ?? throw new InvalidOperationException("PORT_NOT_FOUND");

            var now = DateTime.UtcNow;
            DateTime startAtUtc;
            DateTime endAtUtc;
            decimal depositAmount;

            if (request.Type == BookingType.QuickHold)
            {
                // Giữ chỗ nhanh - bắt đầu ngay
                startAtUtc = now;
                endAtUtc = now.AddMinutes(policy.QuickHoldMinutes);
                depositAmount = policy.QuickHoldFee;

                // Check port available NOW
                var hasConflict = await _db.Bookings.AnyAsync(b =>
                    b.ConnectorPortId == request.PortId &&
                    !b.IsDeleted &&
                    b.Status == BookingStatus.Confirmed &&
                    b.StartAtUtc <= now &&
                    b.EndAtUtc >= now);

                if (hasConflict || port.Status != ConnectorPortStatus.Available)
                    throw new InvalidOperationException("PORT_NOT_AVAILABLE");
            }
            else // Reservation
            {
                if (!request.StartAtUtc.HasValue || !request.DurationMinutes.HasValue)
                    throw new InvalidOperationException("RESERVATION_REQUIRES_TIME");

                var duration = request.DurationMinutes.Value;

                // Validate duration
                if (duration < policy.ReservationMinMinutes || duration > policy.ReservationMaxMinutes)
                    throw new InvalidOperationException("INVALID_DURATION");

                // Must be multiple of block minutes
                if (duration % policy.ReservationBlockMinutes != 0)
                    throw new InvalidOperationException("DURATION_NOT_BLOCK_ALIGNED");

                startAtUtc = request.StartAtUtc.Value;
                endAtUtc = startAtUtc.AddMinutes(duration);

                // Must be in future
                if (startAtUtc <= now.AddMinutes(5))
                    throw new InvalidOperationException("START_TIME_TOO_SOON");

                // Check overlap with existing bookings
                var hasConflict = await _db.Bookings.AnyAsync(b =>
                    b.ConnectorPortId == request.PortId &&
                    !b.IsDeleted &&
                    b.Status == BookingStatus.Confirmed &&
                    b.StartAtUtc < endAtUtc &&
                    b.EndAtUtc > startAtUtc);

                if (hasConflict)
                    throw new InvalidOperationException("TIME_SLOT_OCCUPIED");

                // Calculate fee
                var blocks = duration / 15;
                var isDc = port.Charger?.Type?.Contains("DC") == true;
                var feePerBlock = isDc
                    ? policy.ReservationFeeDcPer15Min
                    : policy.ReservationFeeAcPer15Min;

                depositAmount = blocks * feePerBlock;
            }

            // Create booking
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                Code = GenerateBookingCode(),
                DriverId = driverId,
                ConnectorPortId = request.PortId,
                StartAtUtc = startAtUtc,
                EndAtUtc = endAtUtc,
                Status = BookingStatus.Pending,
                Type = request.Type,
                DepositAmount = depositAmount,
                DepositCurrency = "VND",
                CreatedAt = now
            };

            _db.Bookings.Add(booking);

            // Create payment
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                Amount = depositAmount,
                Currency = "VND",
                Provider = PaymentProvider.Stripe,
                Kind = PaymentKind.Deposit,
                Status = PaymentStatus.Created,
                CreatedAt = now
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            return new BookResult(
                booking.Id,
                booking.Code,
                payment.Id,
                depositAmount,
                "VND"
            );
        }

        public async Task<IReadOnlyList<BookingListItemDto>> GetMyBookingsAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var driverId))
                return Array.Empty<BookingListItemDto>();

            var bookings = await _db.Bookings
                .AsNoTracking()
                .Where(b => b.DriverId == driverId && !b.IsDeleted)
                .Include(b => b.ConnectorPort)
                    .ThenInclude(p => p.Charger)
                    .ThenInclude(c => c.Station)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var result = new List<BookingListItemDto>();

            foreach (var b in bookings)
            {
                var payment = await _db.Payments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.BookingId == b.Id);

                var stationName = b.ConnectorPort?.Charger?.Station?.Name ?? "N/A";
                var holdText = $"{b.StartAtUtc.ToLocalTime():dd/MM HH:mm} - {b.EndAtUtc.ToLocalTime():HH:mm}";

                result.Add(new BookingListItemDto
                {
                    Id = b.Id,
                    BookingCode = b.Code,
                    StationName = stationName,
                    HoldWindowText = holdText,
                    BookingStatus = b.Status,
                    BookingType = b.Type,
                    PaymentStatus = payment?.Status ?? PaymentStatus.Created,
                    Amount = b.DepositAmount,
                    Currency = b.DepositCurrency,
                    PaymentId = payment?.Id ?? Guid.Empty
                });
            }

            return result;
        }

        public async Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(
            Guid portId,
            DateTime fromUtc,
            DateTime toUtc,
            int blockMinutes = 15)
        {
            var policy = await _db.BookingPolicies.FirstOrDefaultAsync();
            if (policy == null) return Array.Empty<AvailableSlotDto>();

            var existingBookings = await _db.Bookings
                .AsNoTracking()
                .Where(b =>
                    b.ConnectorPortId == portId &&
                    !b.IsDeleted &&
                    b.Status == BookingStatus.Confirmed &&
                    b.StartAtUtc < toUtc &&
                    b.EndAtUtc > fromUtc)
                .OrderBy(b => b.StartAtUtc)
                .Select(b => new { b.StartAtUtc, b.EndAtUtc })
                .ToListAsync();

            var slots = new List<AvailableSlotDto>();
            var current = fromUtc;

            while (current < toUtc)
            {
                var slotEnd = current.AddMinutes(blockMinutes);
                if (slotEnd > toUtc) break;

                // Check if slot conflicts with any booking
                var hasConflict = existingBookings.Any(b =>
                    b.StartAtUtc < slotEnd && b.EndAtUtc > current);

                if (!hasConflict)
                {
                    slots.Add(new AvailableSlotDto
                    {
                        StartAtUtc = current,
                        EndAtUtc = slotEnd,
                        DurationMinutes = blockMinutes
                    });
                }

                current = slotEnd;
            }

            return slots;
        }

        public async Task ExpirePendingPaymentsAsync(CancellationToken cancellationToken = default)
        {
            var policy = await _db.BookingPolicies.FirstOrDefaultAsync(cancellationToken);
            if (policy == null) return;

            var cutoff = DateTime.UtcNow.AddMinutes(-policy.PaymentTimeoutMinutes);

            var expiredPayments = await _db.Payments
                .Where(p =>
                    p.Status == PaymentStatus.Created &&
                    p.CreatedAt < cutoff)
                .ToListAsync(cancellationToken);

            foreach (var payment in expiredPayments)
            {
                payment.Status = PaymentStatus.Expired;
                payment.UpdatedAt = DateTime.UtcNow;

                // Cancel associated booking
                if (payment.BookingId.HasValue)
                {
                    var booking = await _db.Bookings.FindAsync(
                        new object[] { payment.BookingId.Value }, 
                        cancellationToken);
                    
                    if (booking != null && booking.Status == BookingStatus.Pending)
                    {
                        booking.Status = BookingStatus.Cancelled;
                        booking.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            if (expiredPayments.Any())
            {
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        private string GenerateBookingCode()
        {
            return $"BK{DateTime.UtcNow:yyMMddHHmmss}{Random.Shared.Next(100, 999)}";
        }
    }
}