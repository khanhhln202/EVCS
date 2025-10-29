using EVCS.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EVCS.Services.Interfaces
{
    public interface IBookingService
    {
        Task<IReadOnlyList<AvailablePortDto>> GetAvailablePortsAsync(Guid chargerId);
        Task<BookResult> CreateBookingAsync(Guid portId, Guid driverId, int holdMinutes);

        Task<List<BookingListItemDto>> GetMyBookingsAsync(string driverId);
    }
}