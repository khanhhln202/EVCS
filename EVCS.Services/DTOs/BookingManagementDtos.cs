using EVCS.Models.Enums;
using System;
using System.Collections.Generic;

namespace EVCS.Services.DTOs
{
    public class BookingManagementListDto
    {
        public List<BookingManagementItemDto> Bookings { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    public class BookingManagementItemDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = "";
        public string DriverName { get; set; } = "";
        public string DriverEmail { get; set; } = "";
        public string StationName { get; set; } = "";
        public string ChargerType { get; set; } = "";
        public int PortIndex { get; set; }
        public DateTime StartAtUtc { get; set; }
        public DateTime EndAtUtc { get; set; }
        public BookingStatus Status { get; set; }
        public BookingType Type { get; set; }
        public decimal DepositAmount { get; set; }
        public string Currency { get; set; } = "VND";
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class BookingDetailDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = "";
        
        // Driver info
        public Guid DriverId { get; set; }
        public string DriverName { get; set; } = "";
        public string DriverEmail { get; set; } = "";
        public string DriverPhone { get; set; } = "";
        
        // Station & Port info
        public Guid StationId { get; set; }
        public string StationName { get; set; } = "";
        public string StationCity { get; set; } = "";
        public string StationAddress { get; set; } = "";
        public Guid ChargerId { get; set; }
        public string ChargerType { get; set; } = "";
        public decimal ChargerMaxPowerKw { get; set; }
        public Guid ConnectorPortId { get; set; }
        public int PortIndex { get; set; }
        public string ConnectorType { get; set; } = "";
        public decimal PricePerKwh { get; set; }
        
        // Booking info
        public DateTime StartAtUtc { get; set; }
        public DateTime EndAtUtc { get; set; }
        public BookingStatus Status { get; set; }
        public BookingType Type { get; set; }
        public decimal DepositAmount { get; set; }
        public string Currency { get; set; } = "VND";
        
        // Payment info
        public Guid? PaymentId { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public DateTime? PaidAtUtc { get; set; }
        public string PaymentProvider { get; set; } = "";
        
        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    public class BookingFilterDto
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        
        public string? SearchTerm { get; set; }
        public BookingStatus? Status { get; set; }
        public BookingType? Type { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public Guid? StationId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class BookingStatsDto
    {
        public int TotalBookings { get; set; }
        public int PendingCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public int ExpiredCount { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}