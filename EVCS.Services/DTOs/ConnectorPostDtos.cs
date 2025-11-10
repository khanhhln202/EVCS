using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.DTOs
{
    public record AvailablePortDto(
        Guid Id,
        int IndexNo,
        string? ConnectorType,
        decimal? MaxPowerKw,
        decimal PricePerKwh
    );
}
