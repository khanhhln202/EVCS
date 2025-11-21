using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Models.Enums
{
    public enum PaymentStatus 
    { 
        Created = 0, 
        Paid = 1,      
        Failed = 2,
        Cancelled = 3,
        Refunded = 4,
        Expired = 5,
        Pending = 6
    }
}
