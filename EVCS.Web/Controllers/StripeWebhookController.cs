using EVCS.Services.Interfaces;
using EVCS.Services.Stripe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

namespace EVCS.Web.Controllers
{
    [Route("api/stripe-webhook")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly string _webhookSecret;

        public StripeWebhookController(IPaymentService paymentService, IOptions<StripeSettings> stripeSettings)
        {
            _paymentService = paymentService;
            _webhookSecret = stripeSettings.Value.WebhookSecret;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _webhookSecret);
                Console.WriteLine($"🔔 StripeWebhook received: {stripeEvent.Type}, Object: {stripeEvent.Data.Object?.GetType().Name}");

                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        if (stripeEvent.Data.Object is PaymentIntent pi)
                            await _paymentService.FulfillPaymentAsync(pi, CancellationToken.None);
                        break;

                    case "charge.succeeded":
                        if (stripeEvent.Data.Object is Charge charge && charge.PaymentIntentId != null)
                        {
                            var piService = new PaymentIntentService();
                            var piFromCharge = await piService.GetAsync(charge.PaymentIntentId);
                            if (piFromCharge != null)
                                await _paymentService.FulfillPaymentAsync(piFromCharge, CancellationToken.None);
                        }
                        break;

                    case "payment_intent.payment_failed":
                        if (stripeEvent.Data.Object is PaymentIntent failedPi)
                            await _paymentService.HandleFailedPaymentAsync(failedPi, CancellationToken.None);
                        break;

                    default:
                        Console.WriteLine($"ℹ Event {stripeEvent.Type} chưa được xử lý");
                        break;
                }

                return Ok();
            }
            catch (StripeException e)
            {
                Console.WriteLine($"❌ StripeException: {e.Message}");
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine($"❌ Exception: {e.Message}");
                return StatusCode(500, e.Message);
            }
        }
    }
}
