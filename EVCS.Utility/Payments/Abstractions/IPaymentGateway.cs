using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Utility.Payments.Abstractions
{
    public interface IPaymentGateway
    {
        string Provider { get; }
        Task<string> CreatePaymentUrlAsync(string orderId, decimal amount, string returnUrl, string ipnUrl);
        Task<bool> VerifyIpnAsync(IDictionary<string, string> payload);
    }
}
