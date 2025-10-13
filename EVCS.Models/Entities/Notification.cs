using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Models.Entities
{
    public class Notification
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }               

        [MaxLength(200)]
        public string Title { get; set; } = default!;
        public string? Body { get; set; }

        // Channel: App|Email|SMS
        [MaxLength(16)]
        public string Channel { get; set; } = "App";

        // Status: Pending|Sent|Delivered|Read
        [MaxLength(16)]
        public string Status { get; set; } = "Pending";

        [MaxLength(64)]
        public string? RelatedEntityType { get; set; }
        public Guid? RelatedEntityId { get; set; }

        public DateTime? SentAt { get; set; }
        public DateTime? ReadAt { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Timestamp] public byte[]? RowVersion { get; set; }
    }
}
