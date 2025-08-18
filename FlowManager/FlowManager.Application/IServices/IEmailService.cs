using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml = false);
        Task SendWelcomeEmailAsync(string email, string firstName);
        Task SendPasswordResetCodeAsync(string email, string firstName, string resetCode);
    }
}