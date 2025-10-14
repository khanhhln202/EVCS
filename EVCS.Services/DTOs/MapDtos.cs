using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.DTOs
{
    public record PortStatusDto(string? ConnectorType, string Status, decimal PricePerKwh);
    public record StationMapItemDto(
    Guid Id, string Code, string Name, string? City,
    double? Lat, double? Lng,
    bool Online, bool? OpenNow,
    IReadOnlyList<PortStatusDto> Ports
    );
}
