using EVCS.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Models.Entities
{
    public class ChargerUnit
    {
        public Guid Id { get; set; }
        public Guid StationId { get; set; }
        public Station Station { get; set; } = default!;


        [Required, MaxLength(64)] public string Name { get; set; } = default!;
        [MaxLength(8)] public string? Type { get; set; } 
        public decimal? MaxPowerKw { get; set; }
        public ChargerStatus Status { get; set; } = ChargerStatus.Online;


        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();


        public ICollection<ConnectorPort> Ports { get; set; } = new List<ConnectorPort>();
    }
}
