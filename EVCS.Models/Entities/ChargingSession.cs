using EVCS.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Models.Entities
{
    public class ChargingSession
    {
        public Guid Id { get; set; }

        public Guid? BookingId { get; set; }       
        public Guid ConnectorPortId { get; set; }  
        public Guid DriverId { get; set; }         
        public Guid? StaffId { get; set; }          

        public DateTime StartedAtUtc { get; set; }
        public DateTime? EndedAtUtc { get; set; }

        public decimal? EnergyKwh { get; set; }       
        public decimal? UnitPricePerKwh { get; set; }   
        public decimal? Subtotal { get; set; }          

        // Started|Ended|Settled
        [MaxLength(16)]
        public string Status { get; set; } = "Started";

        [MaxLength(512)]
        public string? Notes { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Timestamp] public byte[]? RowVersion { get; set; }
    }
}
