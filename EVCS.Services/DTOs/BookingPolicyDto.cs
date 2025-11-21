using System;

namespace EVCS.Services.DTOs
{
    public class BookingPolicyDto
    {
        public Guid Id { get; set; }
        
        // Quick Hold
        public decimal QuickHoldFee { get; set; }
        public int QuickHoldMinutes { get; set; }

        // Reservation
        public decimal ReservationFeeAcPer15Min { get; set; }
        public decimal ReservationFeeDcPer15Min { get; set; }
        public int ReservationMinMinutes { get; set; }
        public int ReservationMaxMinutes { get; set; }
        public int ReservationBlockMinutes { get; set; }

        // Payment
        public int PaymentTimeoutMinutes { get; set; }

        // Legacy
        public decimal AcDeposit { get; set; }
        public decimal DcDeposit { get; set; }
        public int HoldMinutes { get; set; }
        public int CancelFullRefundMinutes { get; set; }
        public int CancelPartialRefundMinutes { get; set; }
        public int NoShowPenaltyPercent { get; set; }
    }
}