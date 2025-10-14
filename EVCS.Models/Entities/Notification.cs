using EVCS.Models.Enums;
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
        [Required, MaxLength(200)] public string Title { get; set; } = default!;
        public string? Body { get; set; }

        public NotificationChannel Channel { get; set; } = NotificationChannel.App;
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

        [MaxLength(64)] public string? RelatedEntityType { get; set; }
        public Guid? RelatedEntityId { get; set; }

        public DateTime? SentAt { get; set; }
        public DateTime? ReadAt { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
