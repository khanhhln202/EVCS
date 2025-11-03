using EVCS.DataAccess.Repository.Interfaces;
using EVCS.Models.Entities;
using EVCS.Models.Enums;
using EVCS.Services.DTOs.Dashboard;
using EVCS.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EVCS.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(IUnitOfWork unitOfWork, ILogger<DashboardService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var now = DateTime.UtcNow;
                var todayStart = now.Date;
                var monthStart = new DateTime(now.Year, now.Month, 1);

                var stats = new DashboardStatsDto();

                // ✅ Tổng quan trạm sạc
                var stations = await _unitOfWork.StationRepo.GetAllAsync(s => !s.IsDeleted);
                stats.TotalStations = stations?.Count() ?? 0;
                stats.OnlineStations = stations?.Count(s => s.Status == StationStatus.Online) ?? 0;

                var chargers = await _unitOfWork.ChargerUnitRepo.GetAllAsync(c => !c.IsDeleted);
                stats.TotalChargers = chargers?.Count() ?? 0;

                var ports = await _unitOfWork.ConnectorPortRepo.GetAllAsync(p => !p.IsDeleted);
                stats.TotalPorts = ports?.Count() ?? 0;
                stats.AvailablePorts = ports?.Count(p => p.Status == ConnectorPortStatus.Available) ?? 0;

                // ✅ Đặt chỗ
                var allBookings = await _unitOfWork.BookingRepo.GetAllAsync(b => !b.IsDeleted);
                stats.TodayBookings = allBookings?.Count(b => b.CreatedAt.Date >= todayStart) ?? 0;
                stats.PendingBookings = allBookings?.Count(b => b.Status == BookingStatus.Pending) ?? 0;
                stats.ConfirmedBookings = allBookings?.Count(b => b.Status == BookingStatus.Confirmed) ?? 0;
                stats.CompletedBookings = allBookings?.Count(b => b.Status == BookingStatus.Completed) ?? 0;

                // ✅ Tài chính
                var payments = await _unitOfWork.PaymentRepo.GetAllAsync(p => p.Status == PaymentStatus.Paid);
                stats.TodayRevenue = payments?
                    .Where(p => p.PaidAtUtc.HasValue && p.PaidAtUtc.Value.Date >= todayStart)
                    .Sum(p => p.Amount) ?? 0;

                stats.MonthRevenue = payments?
                    .Where(p => p.PaidAtUtc.HasValue && p.PaidAtUtc.Value.Date >= monthStart)
                    .Sum(p => p.Amount) ?? 0;

                stats.TotalRevenue = payments?.Sum(p => p.Amount) ?? 0;

                // ✅ Người dùng
                var users = await _unitOfWork.UserRepo.GetAllAsync();
                stats.TotalUsers = users?.Count() ?? 0;
                stats.NewUsersToday = users?.Count(u => u.CreatedAt.Date >= todayStart) ?? 0;
                stats.NewUsersThisMonth = users?.Count(u => u.CreatedAt.Date >= monthStart) ?? 0;

                // ✅ Charts (simplified)
                var last7Days = Enumerable.Range(0, 7).Select(i => todayStart.AddDays(-6 + i)).ToList();

                stats.BookingsTrend = last7Days.Select(date => new ChartDataPoint
                {
                    Label = date.ToString("dd/MM"),
                    Value = allBookings?.Count(b => b.CreatedAt.Date == date) ?? 0
                }).ToList();

                stats.RevenueTrend = last7Days.Select(date => new ChartDataPoint
                {
                    Label = date.ToString("dd/MM"),
                    Value = payments?
                        .Where(p => p.PaidAtUtc.HasValue && p.PaidAtUtc.Value.Date == date)
                        .Sum(p => p.Amount) ?? 0
                }).ToList();

                stats.PortStatusDistribution = ports?
                    .GroupBy(p => p.Status)
                    .Select(g => new PortStatusData
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count()
                    })
                    .ToList() ?? new List<PortStatusData>();

                stats.TopStationsByBookings = new List<TopStationDto>();
                stats.TopStationsByRevenue = new List<TopStationDto>();

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting dashboard stats");
                
                // Return empty DTO instead of throw
                return new DashboardStatsDto();
            }
        }
    }
}