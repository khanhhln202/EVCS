using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Utility.Settings
{
    public class PaymentSettings
    {
        public VNPaySettings VNPay { get; set; } = new();
        public MoMoSettings MoMo { get; set; } = new();

        public class VNPaySettings { public string? TmnCode { get; set; } public string? HashSecret { get; set; } public string? ReturnUrl { get; set; } public string? BaseUrl { get; set; } }
        public class MoMoSettings { public string? PartnerCode { get; set; } public string? AccessKey { get; set; } public string? SecretKey { get; set; } public string? ReturnUrl { get; set; } }
    }
}
