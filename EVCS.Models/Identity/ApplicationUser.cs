using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace EVCS.Models.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public bool IsActive { get; set; } = true; // mapping logic app side
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Profile
        [MaxLength(128)]
        public string? FullName { get; set; }

        // Billing info
        [MaxLength(128)]
        public string? InvoiceDisplayName { get; set; }

        [EmailAddress, MaxLength(256)]
        public string? InvoiceEmail { get; set; }

        [MaxLength(32)]
        public string? TaxId { get; set; } // Mã số thuế (DN)

        [MaxLength(256)]
        public string? InvoiceAddress { get; set; }
    }
}
