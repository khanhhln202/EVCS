using EVCS.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.Interfaces
{
    public interface IBookingService
    {
        Task<Booking> CreateBookingAsync(Guid driverId, Guid portId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);
    }
}
