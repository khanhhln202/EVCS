using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Models.Entities
{
    public class Invoice
    {
        public Guid Id { get; set; }

        [MaxLength(32)]
        public string Number { get; set; } = default!; 

        public Guid DriverId { get; set; }             
        public Guid? SessionId { get; set; }           
        public Guid? BookingId { get; set; }           

        public decimal Total { get; set; }             
        public decimal? Vat { get; set; }              
        [MaxLength(8)]
        public string Currency { get; set; } = "VND";

        public DateTime IssuedAtUtc { get; set; }
        [MaxLength(512)]
        public string? PdfUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
