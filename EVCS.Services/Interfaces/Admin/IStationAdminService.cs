using EVCS.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.Interfaces.Admin
{
    public interface IStationAdminService
    {
        Task<IReadOnlyList<StationListAdminDto>> GetAllAsync(string? city = null);
        Task<StationUpsertDto?> GetAsync(Guid id);
        Task<Guid> CreateAsync(StationUpsertDto dto);
        Task UpdateAsync(StationUpsertDto dto);
        Task SoftDeleteAsync(Guid id);
    }
}
