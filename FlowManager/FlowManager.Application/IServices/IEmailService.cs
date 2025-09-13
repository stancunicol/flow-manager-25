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
        
        // Admin impersonation notifications
        Task SendFormCompletedByAdminEmailAsync(string email, string firstName, string formName, string adminName, DateTime completedAt, string? notes = null);
        Task SendFormApprovedByAdminEmailAsync(string email, string firstName, string formName, string adminName, DateTime approvedAt, string? notes = null);
        Task SendFormRejectedByAdminEmailAsync(string email, string firstName, string formName, string adminName, DateTime rejectedAt, string rejectReason, string? notes = null);
    }
}