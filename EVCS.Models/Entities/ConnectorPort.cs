using EVCS.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Models.Entities
{
    public class ConnectorPort
    {
        public Guid Id { get; set; }
        public Guid ChargerId { get; set; }
        public int IndexNo { get; set; }
        public string? ConnectorType { get; set; }
        public decimal? MaxPowerKw { get; set; }
        public string Status { get; set; } = "Available";
        public decimal DefaultPricePerKwh { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ChargerUnit Charger { get; set; } = default!;
    }
}
