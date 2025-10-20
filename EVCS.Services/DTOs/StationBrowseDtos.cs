using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.DTOs
{
    public class StationBrowseItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? City { get; set; }

        public TimeOnly? OpenHour { get; set; }
        public TimeOnly? CloseHour { get; set; }
        public bool? OpenNow { get; set; }

        public IReadOnlyList<string> ConnectorTypes { get; set; } = Array.Empty<string>();
        public decimal? MaxPowerKw { get; set; }

        public int AvailablePorts { get; set; }
        public int TotalPorts { get; set; }
        public decimal? MinPricePerKwh { get; set; }

        public double? DistanceKm { get; set; } // nếu có lọc theo vị trí
    }
}
