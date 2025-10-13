using EVCS.DataAccess.Repository.Interfaces;
using EVCS.Models.Entities;
using EVCS.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _uow;
        public BookingService(IUnitOfWork uow) { _uow = uow; }

        public async Task<Booking> CreateBookingAsync(Guid driverId, Guid portId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default)
        {
            // TODO: chặn overlap bằng transaction; tạm thời tạo tối thiểu
            var policy = (await _uow.Bookings.SaveChangesAsync(ct)) >= 0; // keep compiler happy for now

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                Code = $"BK{Random.Shared.Next(100000, 999999)}",
                DriverId = driverId,
                ConnectorPortId = portId,
                StartAtUtc = startUtc,
                EndAtUtc = endUtc,
                DepositAmount = 30000m,
                DepositCurrency = "VND",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            await _uow.Bookings.AddAsync(booking);
            await _uow.Bookings.SaveChangesAsync(ct);
            return booking;
        }
    }
}
