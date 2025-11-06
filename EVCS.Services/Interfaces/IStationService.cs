using EVCS.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.Interfaces
{
    public interface IStationService
    {
        Task<IReadOnlyList<StationListItemDto>> GetOnlineStationsAsync(string? city = null);

        Task<IReadOnlyList<StationMapItemDto>> SearchAsync(StationSearchCriteria criteria);

        Task<IReadOnlyList<AvailablePortDto>> GetAvailablePortsAsync(Guid chargerId);
    }
}
