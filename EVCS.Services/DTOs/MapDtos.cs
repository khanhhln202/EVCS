using System;
using System.Collections.Generic;

namespace EVCS.Services.DTOs
{
    public record PortStatusDto(string? ConnectorType, string Status, decimal PricePerKwh);

    // Added Id to identify trụ sạc
    public record ChargerSummaryDto(
        Guid Id,                 
        string? Type,            // AC / DC / GB/T
        decimal? MaxPowerKw,
        int Available,
        int Total,
        decimal? PricePerKwh
    );

    public record StationMapItemDto(
        Guid Id, string Code, string Name, string? City,
        double? Lat, double? Lng,
        bool Online, bool? OpenNow,
        IReadOnlyList<PortStatusDto> Ports
    )
    {
        public string? Hours { get; init; }
        public IReadOnlyList<ChargerSummaryDto>? Chargers { get; init; }
    }
}
