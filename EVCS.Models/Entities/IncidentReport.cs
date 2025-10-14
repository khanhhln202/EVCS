using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Models.Entities
{
    public class IncidentReport
    {
        public Guid Id { get; set; }
        public Guid StationId { get; set; }
        public Guid? ConnectorPortId { get; set; }
        public Guid ReportedByUserId { get; set; }

        [Required, MaxLength(200)] public string Title { get; set; } = default!;
        public string? Description { get; set; }

        [MaxLength(16)] public string Status { get; set; } = "Open";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAtUtc { get; set; }
    }
}
