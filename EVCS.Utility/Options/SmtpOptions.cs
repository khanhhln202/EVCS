using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Utility.Options
{
    public class SmtpOptions
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 25;
        public string? User { get; set; }
        public string? Password { get; set; }
        public bool UseSsl { get; set; }
    }
}
