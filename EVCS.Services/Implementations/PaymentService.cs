using EVCS.DataAccess.Data;
using EVCS.Models.Entities;
using EVCS.Models.Enums;
using EVCS.Services.DTOs;
using EVCS.Services.Interfaces;
using EVCS.Services.Stripe;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _db;
        private readonly string _publishableKey;
        private readonly PaymentIntentService _paymentIntentService; // Dùng service này

        public PaymentService(ApplicationDbContext db, IOptions<StripeSettings> stripeSettings)
        {
            _db = db;
            _publishableKey = stripeSettings.Value.PublishableKey;
            _paymentIntentService = new PaymentIntentService();
        }

        // HÀM 1: CHUẨN BỊ THANH TOÁN
        public async Task<PreparePaymentResponseDto> PreparePaymentAsync(Guid bookingId, string driverId, CancellationToken cancellationToken)
        {
            var booking = await _db.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.DriverId.ToString() == driverId, cancellationToken);

            if (booking == null) throw new Exception("Booking không tìm thấy.");
            if (booking.Status != BookingStatus.Pending) throw new Exception("Booking không ở trạng thái chờ.");

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                BookingId = booking.Id,
                Amount = booking.DepositAmount,
                Currency = booking.DepositCurrency.ToUpper(),
                Provider = PaymentProvider.Stripe,
                Kind = PaymentKind.Deposit,
                Status = PaymentStatus.Created,
            };
            _db.Payments.Add(payment);

            var amountInCents = (long)(booking.DepositAmount * 100);
            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = payment.Currency.ToLower(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true, 
                },
                
                Metadata = new Dictionary<string, string>
            {
                { "PaymentId", payment.Id.ToString() },
                { "BookingId", booking.Id.ToString() }
            }
            };

           
            PaymentIntent paymentIntent = await _paymentIntentService.CreateAsync(options, cancellationToken: cancellationToken);

           
            payment.ProviderRef = paymentIntent.Id; 
            payment.Status = PaymentStatus.Created;

            await _db.SaveChangesAsync(cancellationToken);

           
            return new PreparePaymentResponseDto
            {
                PaymentId = payment.Id,
                ClientSecret = paymentIntent.ClientSecret,
                PublishableKey = _publishableKey,
                Amount = amountInCents,
                Currency = payment.Currency
            };
        }

       
        public async Task FulfillPaymentAsync(PaymentIntent paymentIntent, CancellationToken cancellationToken)
        {
          
            if (!paymentIntent.Metadata.TryGetValue("PaymentId", out var paymentIdString) ||
                !Guid.TryParse(paymentIdString, out var paymentId))
            {
                
                return;
            }

            
            var payment = await _db.Payments.FindAsync(new object[] { paymentId }, cancellationToken);
            if (payment == null) return; 

            
            if (payment.Status == PaymentStatus.Paid) return;

           
            payment.Status = PaymentStatus.Paid;
            payment.PaidAtUtc = DateTime.UtcNow;
            payment.RawPayloadJson = paymentIntent.ToJson(); 

           
            if (payment.BookingId.HasValue)
            {
                var booking = await _db.Bookings.FindAsync(new object[] { payment.BookingId.Value }, cancellationToken);
                if (booking != null)
                {
                    booking.Status = BookingStatus.Confirmed;
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

      
        public async Task HandleFailedPaymentAsync(PaymentIntent paymentIntent, CancellationToken cancellationToken)
        {
            
            if (!paymentIntent.Metadata.TryGetValue("PaymentId", out var paymentIdString) ||
                !Guid.TryParse(paymentIdString, out var paymentId))
            {
                return;
            }

            var payment = await _db.Payments.FindAsync(new object[] { paymentId }, cancellationToken);
            if (payment == null) return;

            
            payment.Status = PaymentStatus.Failed;
            payment.RawPayloadJson = paymentIntent.ToJson();

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
