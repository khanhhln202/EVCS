using EVCS.Models.Enums;
using System;

namespace EVCS.Services.DTOs
{
    public class PaymentViewModel
    {
        public string PublishableKey { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public Guid PaymentId { get; set; }
        public Guid BookingId { get; set; }
        
        // Booking info
        public string BookingCode { get; set; } = string.Empty;
        public BookingType BookingType { get; set; }
        
        // Station & Charger info
        public string StationName { get; set; } = string.Empty;
        public string? StationCity { get; set; }
        public string? ChargerType { get; set; }
        public string PortName { get; set; } = string.Empty;
        
        // Time info
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }
        
        // Payment info
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "VND";

        // Display helpers
        public string BookingTypeDisplay => BookingType switch
        {
            BookingType.QuickHold => "Giữ chỗ nhanh",
            BookingType.Reservation => "Đặt lịch trước",
            _ => "N/A"
        };

        public string TimeRangeDisplay => $"{StartTime:dd/MM/yyyy HH:mm} - {EndTime:HH:mm}";
        
        public string StationFullName => string.IsNullOrEmpty(StationCity) 
            ? StationName 
            : $"{StationName} • {StationCity}";
    }
}