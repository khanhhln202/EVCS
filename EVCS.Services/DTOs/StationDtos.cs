using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.DTOs
{
    public record StationListItemDto(Guid Id, string Code, string Name, string? City, bool Online);

    public class StationSearchCriteria
    {
        public string? City { get; set; }
        public string? ConnectorType { get; set; } // legacy
        public bool? OpenNow { get; set; }

        // New: Charger type and power range filters
        public string? ChargerType { get; set; }    // AC / DC / GB/T
        public decimal? MinPowerKw { get; set; }    // e.g. 3.5
        public decimal? MaxPowerKw { get; set; }    // e.g. 180

        // Mở rộng cho “nearby”
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? RadiusKm { get; set; }
    }
}
