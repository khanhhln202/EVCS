using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Models.Entities
{
    public class BookingPolicy
    {
        public Guid Id { get; set; }
        
        // Quick Hold
        public decimal QuickHoldFee { get; set; } = 30000m;
        public int QuickHoldMinutes { get; set; } = 15;

        // Reservation
        public decimal ReservationFeeAcPer15Min { get; set; } = 5000m;
        public decimal ReservationFeeDcPer15Min { get; set; } = 10000m;
        public int ReservationMinMinutes { get; set; } = 30;
        public int ReservationMaxMinutes { get; set; } = 180;
        public int ReservationBlockMinutes { get; set; } = 15; // Khối thời gian tối thiểu

        // Payment timeout
        public int PaymentTimeoutMinutes { get; set; } = 5;

        // Legacy (giữ lại để không ảnh hưởng code cũ)
        public decimal AcDeposit { get; set; } = 30000m;
        public decimal DcDeposit { get; set; } = 50000m;
        public int HoldMinutes { get; set; } = 15;
        public int CancelFullRefundMinutes { get; set; } = 30;
        public int CancelPartialRefundMinutes { get; set; } = 30;
        public int NoShowPenaltyPercent { get; set; } = 100;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
