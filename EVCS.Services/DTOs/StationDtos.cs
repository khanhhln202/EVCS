using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.DTOs
{
    public record StationListItemDto(Guid Id, string Code, string Name, string? City, bool Online);
}
