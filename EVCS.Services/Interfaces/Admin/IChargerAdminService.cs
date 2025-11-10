using EVCS.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.Interfaces.Admin
{
    public interface IChargerAdminService
    {
        Task<IReadOnlyList<ChargerUnitUpsertDto>> GetByStationAsync(Guid stationId);
        Task<ChargerUnitUpsertDto?> GetAsync(Guid id);
        Task<Guid> CreateAsync(ChargerUnitUpsertDto dto);
        Task UpdateAsync(ChargerUnitUpsertDto dto);
        Task SoftDeleteAsync(Guid id);
    }
}
