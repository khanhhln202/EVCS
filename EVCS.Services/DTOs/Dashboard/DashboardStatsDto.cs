using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        // Tổng quan
        public int TotalStations { get; set; }
        public int OnlineStations { get; set; }
        public int TotalChargers { get; set; }
        public int TotalPorts { get; set; }
        public int AvailablePorts { get; set; }

        // Đặt chỗ
        public int TodayBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CompletedBookings { get; set; }

        // Tài chính
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public decimal TotalRevenue { get; set; }

        // Người dùng
        public int TotalUsers { get; set; }
        public int NewUsersToday { get; set; }
        public int NewUsersThisMonth { get; set; }

        // Top stations
        public List<TopStationDto> TopStationsByBookings { get; set; } = new();
        public List<TopStationDto> TopStationsByRevenue { get; set; } = new();

        // Biểu đồ
        public List<ChartDataPoint> BookingsTrend { get; set; } = new();
        public List<ChartDataPoint> RevenueTrend { get; set; } = new();
        public List<PortStatusData> PortStatusDistribution { get; set; } = new();
    }

    public class TopStationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? City { get; set; }
        public int BookingsCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class PortStatusData
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
