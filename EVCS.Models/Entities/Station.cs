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
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? TimezoneId { get; set; }
        public Point? Location { get; set; } // geography
        public TimeSpan? OpenHour { get; set; }
        public TimeSpan? CloseHour { get; set; }
        public string Status { get; set; } = "Online"; // Online|Offline
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<ChargerUnit> Chargers { get; set; } = new List<ChargerUnit>();
    }
}
