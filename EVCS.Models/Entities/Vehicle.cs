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
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? PlateNumber { get; set; }
        public decimal? BatteryCapacityKwh { get; set; }
        public string? ConnectorTypes { get; set; } // CSV đơn giản


        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
