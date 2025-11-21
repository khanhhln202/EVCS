using EVCS.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EVCS.Services.Interfaces
{
    public interface IBookingService
    {
        Task<IReadOnlyList<PortAvailabilityDto>> GetAvailablePortsAsync(Guid chargerId);
        
        Task<BookResult> CreateBookingAsync(CreateBookingRequestDto request, Guid driverId);
        
        Task<IReadOnlyList<BookingListItemDto>> GetMyBookingsAsync(string userId);
        
        Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(
            Guid portId, 
            DateTime fromUtc, 
            DateTime toUtc, 
            int blockMinutes = 15);
        
        Task ExpirePendingPaymentsAsync(CancellationToken cancellationToken = default);
        
        Task<BookingDetail?> GetBookingDetailsAsync(Guid bookingId);
        Task<CancelBookingResult> CancelBookingAsync(Guid bookingId, Guid driverId);
    }
}