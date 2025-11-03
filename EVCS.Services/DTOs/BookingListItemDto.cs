using EVCS.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.DTOs
{
    public class BookingListItemDto
    {
        public Guid Id { get; set; } // Dùng cho link "Chi tiết"
        public string BookingCode { get; set; } = "";
        public string StationName { get; set; } = "";
        public string HoldWindowText { get; set; } = ""; // Vd: "08:00 - 08:15 21/10"
        public BookingStatus BookingStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public Guid PaymentId { get; set; } // Dùng cho link "Thanh toán"
    }
}
