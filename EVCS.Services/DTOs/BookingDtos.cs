using System;

namespace EVCS.Services.DTOs
{
    public record AvailablePortDto(
        Guid Id,
        int IndexNo,
        string? ConnectorType,
        decimal? MaxPowerKw,
        decimal PricePerKwh
    );

    public record BookResult(
        Guid BookingId,
        string BookingCode,
        Guid PaymentId,
        decimal DepositAmount,
        string Currency
    );
}