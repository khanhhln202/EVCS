using EVCS.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.DTOs
{


    public record StationListAdminDto(Guid Id, string Code, string Name, string? City, bool Online, int Chargers, int Ports);


    public class StationUpsertDto
    {
        public Guid Id { get; set; }
        [Required, MaxLength(32)] public string Code { get; set; } = string.Empty;
        [Required, MaxLength(128)] public string Name { get; set; } = string.Empty;
        [MaxLength(256)] public string? Address { get; set; }
        [MaxLength(64)] public string? City { get; set; }
        [MaxLength(64)] public string? TimezoneId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public TimeOnly? OpenHour { get; set; }
        public TimeOnly? CloseHour { get; set; }
        public StationStatus Status { get; set; } = StationStatus.Online;
        public byte[]? RowVersion { get; set; }
    }


    public class ChargerUnitUpsertDto
    {
        public Guid Id { get; set; }
        [Required] public Guid StationId { get; set; }
        [Required, MaxLength(64)] public string Name { get; set; } = string.Empty;
        [MaxLength(8)] public string? Type { get; set; } // AC/DC
        public decimal? MaxPowerKw { get; set; }
        public ChargerStatus Status { get; set; } = ChargerStatus.Online;
        public byte[]? RowVersion { get; set; }
    }


    public class ConnectorPortUpsertDto
    {
        public Guid Id { get; set; }
        [Required] public Guid ChargerId { get; set; }
        [Range(1, 64)] public int IndexNo { get; set; }
        [MaxLength(16)] public string? ConnectorType { get; set; }
        public decimal? MaxPowerKw { get; set; }
        [Range(0, 99999999999999.99)] public decimal DefaultPricePerKwh { get; set; }
        public ConnectorPortStatus Status { get; set; } = ConnectorPortStatus.Available;
        public byte[]? RowVersion { get; set; }
    }


    public class BookingPolicyDto
    {
        public Guid Id { get; set; }
        public decimal AcDeposit { get; set; }
        public decimal DcDeposit { get; set; }
        public int HoldMinutes { get; set; }
        public int CancelFullRefundMinutes { get; set; }
        public int CancelPartialRefundMinutes { get; set; }
        public int NoShowPenaltyPercent { get; set; }
    }
}
