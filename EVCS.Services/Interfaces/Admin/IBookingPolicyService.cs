using EVCS.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.Interfaces.Admin
{
    public interface IBookingPolicyService
    {
        Task<BookingPolicyDto> GetCurrentAsync();
        Task UpdateAsync(BookingPolicyDto dto);
    }
}
