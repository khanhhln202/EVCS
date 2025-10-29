using EVCS.Services.DTOs;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.Interfaces
{
        public class PreparePaymentResponseDto
        {
            public string ClientSecret { get; set; } = default!;
            public Guid PaymentId { get; set; }
            public string PublishableKey { get; set; } = default!;
            public long Amount { get; set; }
            public string Currency { get; set; } = default!;
        }

        public interface IPaymentService
        {
           
            Task<PreparePaymentResponseDto> PreparePaymentAsync(Guid bookingId, string driverId, CancellationToken cancellationToken);

           
            Task FulfillPaymentAsync(PaymentIntent paymentIntent, CancellationToken cancellationToken);

           
            Task HandleFailedPaymentAsync(PaymentIntent paymentIntent, CancellationToken cancellationToken);
            


    }
}

