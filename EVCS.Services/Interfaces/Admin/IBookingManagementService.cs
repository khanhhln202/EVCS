using EVCS.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EVCS.Services.Interfaces.Admin
{
    public interface IBookingManagementService
    {
        Task<BookingManagementListDto> GetBookingsAsync(BookingFilterDto filter);
        Task<BookingDetailDto> GetBookingDetailAsync(Guid bookingId);
        Task CancelBookingAsync(Guid bookingId, string reason);
        Task CompleteBookingAsync(Guid bookingId);
        Task<BookingStatsDto> GetStatsAsync();
    }
}