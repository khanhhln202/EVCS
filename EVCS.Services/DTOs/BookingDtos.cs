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

    public class BookingDetail
    {
        public Guid Id { get; set; }
        public string BookingCode { get; set; } = string.Empty;

        // Thông tin trạm
        public string StationName { get; set; } = string.Empty;
        public string? StationAddress { get; set; }

        // Thông tin charger & port
        public string ChargerName { get; set; } = string.Empty;
        public int PortIndex { get; set; }
        public string? ConnectorType { get; set; }

        // Thông tin booking
        public BookingType BookingType { get; set; }
        public BookingStatus BookingStatus { get; set; }
        public string HoldWindowText { get; set; } = string.Empty;
        public DateTime? StartAtUtc { get; set; }
        public DateTime? EndAtUtc { get; set; }
        public int? DurationMinutes { get; set; }

        // Thông tin thanh toán
        public PaymentStatus PaymentStatus { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public Guid? PaymentId { get; set; }

        // Thông tin driver (để check quyền)
        public string DriverId { get; set; } = string.Empty;

        // Timestamp
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CancelBookingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }

        public static CancelBookingResult SuccessResult(string message = "Hủy đặt chỗ thành công")
        {
            return new CancelBookingResult
            {
                Success = true,
                Message = message
            };
        }

        public static CancelBookingResult FailureResult(string message, string? errorCode = null)
        {
            return new CancelBookingResult
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }
}