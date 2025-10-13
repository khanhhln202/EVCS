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
