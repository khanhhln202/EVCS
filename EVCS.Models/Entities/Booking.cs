using EVCS.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Models.Entities
{
    public class Booking
    {
        public Guid Id { get; set; }
        [Required, MaxLength(24)] public string Code { get; set; } = default!;

        public Guid DriverId { get; set; }
        public Guid ConnectorPortId { get; set; }

        public DateTime StartAtUtc { get; set; }
        public DateTime EndAtUtc { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public BookingType Type { get; set; }

        public decimal DepositAmount { get; set; }
        [MaxLength(8)] public string DepositCurrency { get; set; } = "VND";

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public ConnectorPort ConnectorPort { get; set; } = default!;
    }
}
