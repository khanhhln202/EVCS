using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.DTOs.Profile
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? InvoiceDisplayName { get; set; }
        public string? InvoiceEmail { get; set; }
        public string? InvoiceAddress { get; set; }
        public string? TaxId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
    }
}
