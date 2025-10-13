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
        public string Name { get; set; } = default!;
        public string? Type { get; set; } // AC/DC
        public decimal? MaxPowerKw { get; set; }
        public string Status { get; set; } = "Online";
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Station Station { get; set; } = default!;
        public ICollection<ConnectorPort> Ports { get; set; } = new List<ConnectorPort>();
    }
}
