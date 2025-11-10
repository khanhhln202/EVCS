using EVCS.DataAccess.Data;
using EVCS.Models.Enums;
using EVCS.Services.DTOs;
using EVCS.Services.Interfaces.Admin;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EVCS.Services.Implementations
{
    public class BookingManagementService : IBookingManagementService
    {
        private readonly ApplicationDbContext _db;

        public BookingManagementService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<BookingManagementListDto> GetBookingsAsync(BookingFilterDto filter)
        {
            var query = _db.Bookings
                .Include(b => b.ConnectorPort)
                    .ThenInclude(p => p.Charger)
                    .ThenInclude(c => c.Station)
                .Where(b => !b.IsDeleted);

            // Search
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(b =>
                    b.Code.ToLower().Contains(term) ||
                    b.ConnectorPort.Charger.Station.Name.ToLower().Contains(term));
            }

            // Filters
            if (filter.Status.HasValue)
                query = query.Where(b => b.Status == filter.Status.Value);

            if (filter.Type.HasValue)
                query = query.Where(b => b.Type == filter.Type.Value);

            if (filter.StationId.HasValue)
                query = query.Where(b => b.ConnectorPort.Charger.StationId == filter.StationId.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(b => b.StartAtUtc >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(b => b.EndAtUtc <= filter.ToDate.Value);

            var totalCount = await query.CountAsync();

            var bookings = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(b => new BookingManagementItemDto
                {
                    Id = b.Id,
                    Code = b.Code,
                    DriverName = _db.Users.Where(u => u.Id == b.DriverId).Select(u => u.FullName ?? u.Email).FirstOrDefault() ?? "N/A",
                    DriverEmail = _db.Users.Where(u => u.Id == b.DriverId).Select(u => u.Email).FirstOrDefault() ?? "N/A",
                    StationName = b.ConnectorPort.Charger.Station.Name,
                    ChargerType = b.ConnectorPort.Charger.Type,
                    PortIndex = b.ConnectorPort.IndexNo,
                    StartAtUtc = b.StartAtUtc,
                    EndAtUtc = b.EndAtUtc,
                    Status = b.Status,
                    Type = b.Type,
                    DepositAmount = b.DepositAmount,
                    Currency = b.DepositCurrency,
                    PaymentStatus = _db.Payments
                        .Where(p => p.BookingId == b.Id)
                        .OrderByDescending(p => p.CreatedAt)
                        .Select(p => p.Status)
                        .FirstOrDefault(),
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            return new BookingManagementListDto
            {
                Bookings = bookings,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<BookingDetailDto> GetBookingDetailAsync(Guid bookingId)
        {
            var booking = await _db.Bookings
                .Include(b => b.ConnectorPort)
                    .ThenInclude(p => p.Charger)
                    .ThenInclude(c => c.Station)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                throw new InvalidOperationException("Booking không tồn tại.");

            var driver = await _db.Users.FindAsync(booking.DriverId);
            var payment = await _db.Payments
                .Where(p => p.BookingId == bookingId)
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            return new BookingDetailDto
            {
                Id = booking.Id,
                Code = booking.Code,

                DriverId = booking.DriverId,
                DriverName = driver?.FullName ?? "N/A",
                DriverEmail = driver?.Email ?? "N/A",
                DriverPhone = driver?.PhoneNumber ?? "N/A",

                StationId = booking.ConnectorPort.Charger.StationId,
                StationName = booking.ConnectorPort.Charger.Station.Name,
                StationCity = booking.ConnectorPort.Charger.Station.City ?? "",
                StationAddress = booking.ConnectorPort.Charger.Station.Address ?? "",

                ChargerId = booking.ConnectorPort.ChargerId,
                ChargerType = booking.ConnectorPort.Charger.Type,
                ChargerMaxPowerKw = booking.ConnectorPort.Charger.MaxPowerKw ?? 0,

                ConnectorPortId = booking.ConnectorPortId,
                PortIndex = booking.ConnectorPort.IndexNo,
                ConnectorType = booking.ConnectorPort.ConnectorType ?? "",
                PricePerKwh = booking.ConnectorPort.DefaultPricePerKwh,

                StartAtUtc = booking.StartAtUtc,
                EndAtUtc = booking.EndAtUtc,
                Status = booking.Status,
                Type = booking.Type,
                DepositAmount = booking.DepositAmount,
                Currency = booking.DepositCurrency,

                PaymentId = payment?.Id,
                PaymentStatus = payment?.Status,
                PaidAtUtc = payment?.PaidAtUtc,
                PaymentProvider = payment?.Provider.ToString() ?? "",

                CreatedAt = booking.CreatedAt,
                UpdatedAt = booking.UpdatedAt,
                DeletedAt = booking.DeletedAt
            };
        }

        public async Task CancelBookingAsync(Guid bookingId, string reason)
        {
            var booking = await _db.Bookings.FindAsync(bookingId);
            if (booking == null)
                throw new InvalidOperationException("Booking không tồn tại.");

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
                throw new InvalidOperationException("Không thể hủy booking ở trạng thái này.");

            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.UtcNow;

            // TODO: Hoàn tiền nếu cần (tùy chính sách)

            await _db.SaveChangesAsync();
        }

        public async Task CompleteBookingAsync(Guid bookingId)
        {
            var booking = await _db.Bookings.FindAsync(bookingId);
            if (booking == null)
                throw new InvalidOperationException("Booking không tồn tại.");

            if (booking.Status != BookingStatus.Confirmed)
                throw new InvalidOperationException("Chỉ có thể hoàn thành booking đã xác nhận.");

            booking.Status = BookingStatus.Completed;
            booking.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        public async Task<BookingStatsDto> GetStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var bookings = await _db.Bookings
                .Where(b => !b.IsDeleted)
                .ToListAsync();

            var todayPayments = await _db.Payments
                .Where(p => p.PaidAtUtc.HasValue &&
                           p.PaidAtUtc.Value >= today &&
                           p.PaidAtUtc.Value < tomorrow &&
                           p.Status == PaymentStatus.Paid)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var totalPayments = await _db.Payments
                .Where(p => p.Status == PaymentStatus.Paid)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            return new BookingStatsDto
            {
                TotalBookings = bookings.Count,
                PendingCount = bookings.Count(b => b.Status == BookingStatus.Pending),
                ConfirmedCount = bookings.Count(b => b.Status == BookingStatus.Confirmed),
                CompletedCount = bookings.Count(b => b.Status == BookingStatus.Completed),
                CancelledCount = bookings.Count(b => b.Status == BookingStatus.Cancelled),
                ExpiredCount = bookings.Count(b => b.Status == BookingStatus.Expired),
                TodayRevenue = todayPayments,
                TotalRevenue = totalPayments
            };
        }
    }
}