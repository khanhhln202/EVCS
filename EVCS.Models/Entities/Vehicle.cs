using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Models.Entities
{
    public class Vehicle
    {
        public Guid Id { get; set; }

        public Guid DriverId { get; set; }              

        [MaxLength(64)] public string? Brand { get; set; }
        [MaxLength(64)] public string? Model { get; set; }
        [MaxLength(32)] public string? PlateNumber { get; set; }

        public decimal? BatteryCapacityKwh { get; set; } // decimal(18,3)

        [MaxLength(128)] public string? ConnectorTypes { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
