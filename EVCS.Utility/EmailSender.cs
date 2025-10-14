using EVCS.Utility.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Utility
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string body);
    }


    public class EmailSender : IEmailSender
    {
        private readonly SmtpOptions _opt;
        public EmailSender(IOptions<SmtpOptions> opt) { _opt = opt.Value; }
        public Task SendAsync(string to, string subject, string body)
        {
            // TODO: implement SMTP or provider SDK
            return Task.CompletedTask;
        }
    }
}
