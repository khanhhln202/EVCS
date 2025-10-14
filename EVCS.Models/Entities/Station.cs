using EVCS.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Models.Entities
{
    public class Station
    {
        public Guid Id { get; set; }


        [Required, MaxLength(32)]
        public string Code { get; set; } = default!;


        [Required, MaxLength(128)]
        public string Name { get; set; } = default!;


        [MaxLength(256)] public string? Address { get; set; }
        [MaxLength(64)] public string? City { get; set; }
        [MaxLength(64)] public string? TimezoneId { get; set; }


        // Sử dụng NetTopologySuite hoặc WKT string; map đơn giản trước
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public TimeOnly? OpenHour { get; set; }
        public TimeOnly? CloseHour { get; set; }

        public StationStatus Status { get; set; } = StationStatus.Online;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public ICollection<ChargerUnit> Chargers { get; set; } = new List<ChargerUnit>();
    }
}
