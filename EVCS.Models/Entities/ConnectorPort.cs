using EVCS.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Models.Entities
{
    public class ConnectorPort
    {
        public Guid Id { get; set; }
        public Guid ChargerId { get; set; }
        public ChargerUnit Charger { get; set; } = default!;

        public int IndexNo { get; set; }
        [MaxLength(16)] public string? ConnectorType { get; set; }
        public decimal? MaxPowerKw { get; set; }
        public ConnectorPortStatus Status { get; set; } = ConnectorPortStatus.Available;

        [Range(0, 99999999999999.99)]
        public decimal DefaultPricePerKwh { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
