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
        public PaymentStatus PaymentStatus { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public Guid PaymentId { get; set; } 
    }
}