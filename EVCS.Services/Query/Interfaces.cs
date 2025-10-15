using EVCS.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.Query
{
    public class StationSearchCriteria
    {
        public string? City { get; set; }
        public string? ConnectorType { get; set; }
        public bool? OpenNow { get; set; }
    }


    public interface IStationQueryService
    {
        Task<IReadOnlyList<StationMapItemDto>> SearchAsync(StationSearchCriteria criteria);
    }
}
