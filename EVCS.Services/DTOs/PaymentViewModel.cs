
namespace EVCS.Services.DTOs
{
    public class PaymentViewModel
    {
        public string PublishableKey { get; set; }
        public string ClientSecret { get; set; }
        public Guid PaymentId { get; set; }
        public decimal TotalAmount { get; set; }
        public string BookingCode { get; set; }
        public string StationName { get; set; }
        public string PortName { get; set; }
        public int Minutes { get; set; }
    }
}