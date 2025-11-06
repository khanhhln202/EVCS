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

        public PaymentProvider Provider { get; set; }
        public PaymentKind Kind { get; set; }

        [Range(0, double.MaxValue)] public decimal Amount { get; set; }
        [MaxLength(8)] public string Currency { get; set; } = "VND";

        public PaymentStatus Status { get; set; } = PaymentStatus.Created;
        [MaxLength(128)] public string? ProviderRef { get; set; }
        public DateTime? PaidAtUtc { get; set; }
        public string? RawPayloadJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public Booking? Booking { get; set; }
    }
}
