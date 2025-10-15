using EVCS.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.Admin
{
    public interface IStationAdminService
    {
        Task<IReadOnlyList<StationListAdminDto>> GetAllAsync(string? city = null);
        Task<StationUpsertDto?> GetAsync(Guid id);
        Task<Guid> CreateAsync(StationUpsertDto dto);
        Task UpdateAsync(StationUpsertDto dto);
        Task SoftDeleteAsync(Guid id);
    }


    public interface IChargerAdminService
    {
        Task<IReadOnlyList<ChargerUnitUpsertDto>> GetByStationAsync(Guid stationId);
        Task<ChargerUnitUpsertDto?> GetAsync(Guid id);
        Task<Guid> CreateAsync(ChargerUnitUpsertDto dto);
        Task UpdateAsync(ChargerUnitUpsertDto dto);
        Task SoftDeleteAsync(Guid id);
    }


    public interface IConnectorPortAdminService
    {
        Task<IReadOnlyList<ConnectorPortUpsertDto>> GetByChargerAsync(Guid chargerId);
        Task<ConnectorPortUpsertDto?> GetAsync(Guid id);
        Task<Guid> CreateAsync(ConnectorPortUpsertDto dto);
        Task UpdateAsync(ConnectorPortUpsertDto dto);
        Task SoftDeleteAsync(Guid id);
    }


    public interface IBookingPolicyService
    {
        Task<BookingPolicyDto> GetCurrentAsync();
        Task UpdateAsync(BookingPolicyDto dto);
    }
}
