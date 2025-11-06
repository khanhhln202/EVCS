using EVCS.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.Interfaces.Admin
{
    public interface IConnectorPortAdminService
    {
        Task<IReadOnlyList<ConnectorPortUpsertDto>> GetByChargerAsync(Guid chargerId);
        Task<ConnectorPortUpsertDto?> GetAsync(Guid id);
        Task<Guid> CreateAsync(ConnectorPortUpsertDto dto);
        Task UpdateAsync(ConnectorPortUpsertDto dto);
        Task SoftDeleteAsync(Guid id);
    }
}
