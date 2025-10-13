using EVCS.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Models.Entities
{
    public class Payment
    {
        public Guid Id { get; set; }

        public Guid? BookingId { get; set; }        
        public Guid? SessionId { get; set; }        

        [MaxLength(16)]
        public string Provider { get; set; } = "VNPay";

        // Kind: Deposit | Settlement | Refund
        [MaxLength(16)]
        public string Kind { get; set; } = "Deposit";

        public decimal Amount { get; set; }         
        [MaxLength(8)]
        public string Currency { get; set; } = "VND";

        // Created | Paid | Failed | Expired
        [MaxLength(16)]
        public string Status { get; set; } = "Created";

        [MaxLength(128)]
        public string? ProviderRef { get; set; }    // unique w/ provider when not null

        public DateTime? PaidAtUtc { get; set; }
        public string? RawPayloadJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Timestamp] public byte[]? RowVersion { get; set; }
    }
}
