using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBQHub.Application.Common.Interfaces
{
    public interface IEmailSender
    {
        Task SendTestMailAsync(string toEmail);
        Task SendMailAsync(string toEmail, string subject, string bodyHtml, string? fromEmail = null);
    }

}
