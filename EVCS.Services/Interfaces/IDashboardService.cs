using EVCS.Services.DTOs.Dashboard;

namespace EVCS.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}