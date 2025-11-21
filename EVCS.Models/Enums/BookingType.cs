using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Models.Enums
{
    public enum BookingType
    {
        QuickHold = 1,      // Giữ chỗ nhanh (15 phút)
        Reservation = 2     // Đặt lịch trước (30-180 phút)
    }
}
