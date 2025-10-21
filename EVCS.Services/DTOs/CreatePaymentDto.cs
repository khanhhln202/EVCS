using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.DTOs
{
    public class CreatePaymentRequestDto
    {
        [Required]
        public Guid BookingId { get; set; }
    }

    public class CreatePaymentResponseDto
    {
        public string CheckoutUrl { get; set; }
        public Guid PaymentId { get; set; }
    }
}
