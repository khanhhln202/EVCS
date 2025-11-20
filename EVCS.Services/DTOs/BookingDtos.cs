using EVCS.Models.Enums;
using System;

namespace EVCS.Services.DTOs
{
    public record BookResult(
        Guid BookingId,
        string BookingCode,
        Guid PaymentId,
        decimal DepositAmount,
        string Currency
    );

    public class BookingListItemDto
    {
        public Guid Id { get; set; } 
        public string BookingCode { get; set; } = "";
        public string StationName { get; set; } = "";
        public string HoldWindowText { get; set; } = ""; 
        public BookingStatus BookingStatus { get; set; }
        public BookingType BookingType { get; set; } // Mới thêm
        public PaymentStatus PaymentStatus { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public Guid PaymentId { get; set; } 
    }

    // DTO cho available ports - THÊM MỚI
    public class PortAvailabilityDto
    {
        public Guid Id { get; set; }
        public int IndexNo { get; set; }
        public string? ConnectorType { get; set; }
        public decimal PricePerKwh { get; set; }
    }

    // DTO cho request đặt chỗ mới
    public class CreateBookingRequestDto
    {
        public Guid PortId { get; set; }
        public BookingType Type { get; set; }
        
        // Cho Reservation
        public DateTime? StartAtUtc { get; set; }
        public int? DurationMinutes { get; set; }
    }

    // DTO trả về khi check available slots
    public class AvailableSlotDto
    {
        public DateTime StartAtUtc { get; set; }
        public DateTime EndAtUtc { get; set; }
        public int DurationMinutes { get; set; }
    }
}