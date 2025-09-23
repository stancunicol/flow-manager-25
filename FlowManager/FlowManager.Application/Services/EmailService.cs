using FlowManager.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace FlowManager.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isBodyHtml = false)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");

                using var client = new SmtpClient(smtpSettings["Host"], int.Parse(smtpSettings["Port"]))
                {
                    Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"])
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["FromEmail"], smtpSettings["FromName"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isBodyHtml
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email to {toEmail}: {ex.Message}");
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string email, string firstName)
        {
            var subject = "Your account has been created -  FMST";

            var body = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                .content {{ padding: 20px; }}
                .warning {{ 
                    background-color: #fff3cd; 
                    border: 1px solid #ffeaa7; 
                    padding: 15px; 
                    margin: 20px 0; 
                    border-radius: 5px;
                }}
                .footer {{ 
                    background-color: #f8f9fa; 
                    padding: 15px; 
                    text-align: center; 
                    font-size: 12px; 
                    color: #6c757d;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>Welcome to FMST!</h2>
                </div>
        
                <div class='content'>
                    <p>Hello <strong>{firstName}</strong>,</p>
            
                    <p>Your account has been successfully created in the Flow Manager System Template.</p>
            
                    <p><strong>Account details:</strong></p>
                    <ul>
                        <li><strong>Email:</strong> {email}</li>
                    </ul>
            
                    <p><strong>Steps to access your account:</strong></p>
                    <ol>
                        <li>Access the application and click on <strong>Reset password</strong> in the bottom-right corner</li>
                        <li>Enter the email address on which you received this message</li>
                        <li>Click on the <strong>Send Reset Code</strong> button</li>
                        <li>Enter the code received along with your new password</li>
                        <li>After redirection, enter your email and new password on the login page</li>
                    </ol>

                    <div class='warning'>
                        <strong>Important:</strong> You must set up your password before you can access the system.
                    </div>
            
                    <p>If you have any questions or issues, please contact the system administrator.</p>
            
                    <p>Best regards,<br>The FMST Administration Team</p>
                </div>
        
                <div class='footer'>
                    <p>This email was generated automatically. Please do not reply to this message.</p>
                </div>
            </div>
        </body>
        </html>";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendPasswordResetCodeAsync(string email, string firstName, string resetCode)
        {
            var subject = "Password Reset Code - FMST";

            var body = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                .content {{ padding: 20px; }}
                .code-box {{ 
                    background-color: #e9ecef; 
                    border: 1px solid #ced4da; 
                    padding: 15px; 
                    margin: 20px 0; 
                    font-family: monospace; 
                    font-size: 24px;
                    text-align: center;
                    border-radius: 5px;
                    letter-spacing: 2px;
                }}
                .warning {{ 
                    background-color: #fff3cd; 
                    border: 1px solid #ffeaa7; 
                    padding: 15px; 
                    margin: 20px 0; 
                    border-radius: 5px;
                }}
                .footer {{ 
                    background-color: #f8f9fa; 
                    padding: 15px; 
                    text-align: center; 
                    font-size: 12px; 
                    color: #6c757d;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>Password Reset - FMST</h2>
                </div>
        
                <div class='content'>
                    <p>Hello <strong>{firstName}</strong>,</p>
            
                    <p>You have requested to reset your password.</p>
            
                    <p><strong>Your verification code:</strong></p>
            
                    <div class='code-box'>
                        {resetCode}
                    </div>
            
                    <div class='warning'>
                        <strong>Important:</strong> This code is valid for 15 minutes only.
                    </div>
            
                    <p>Use this code on the reset password page to set your new password.</p>
            
                    <p>If you did not request this password reset, please ignore this email.</p>
            
                    <p>Best regards,<br>The FMST Team</p>
                </div>
        
                <div class='footer'>
                    <p>This email was generated automatically. Please do not reply to this message.</p>
                </div>
            </div>
        </body>
        </html>";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendFormCompletedByAdminEmailAsync(string email, string firstName, string formName, string adminName, DateTime completedAt, string? notes = null)
        {
            var subject = "Form Completed by Admin - FMST";

            var notesSection = !string.IsNullOrEmpty(notes) 
                ? $@"<p><strong>Admin Notes:</strong></p>
                     <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #007bff; margin: 15px 0;'>
                         {notes}
                     </div>" 
                : "";

            var body = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                .content {{ padding: 20px; }}
                .success-box {{ 
                    background-color: #d4edda; 
                    border: 1px solid #c3e6cb; 
                    padding: 15px; 
                    margin: 20px 0; 
                    border-radius: 5px;
                    border-left: 4px solid #28a745;
                }}
                .footer {{ 
                    background-color: #f8f9fa; 
                    padding: 15px; 
                    text-align: center; 
                    font-size: 12px; 
                    color: #6c757d;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>Form Completed - FMST</h2>
                </div>
        
                <div class='content'>
                    <p>Hello <strong>{firstName}</strong>,</p>
            
                    <div class='success-box'>
                        <strong>✅ Your form has been completed by admin</strong>
                    </div>
            
                    <p><strong>Form Details:</strong></p>
                    <ul>
                        <li><strong>Form:</strong> {formName}</li>
                        <li><strong>Completed by:</strong> {adminName}</li>
                        <li><strong>Completed at:</strong> {completedAt:dd/MM/yyyy HH:mm}</li>
                    </ul>

                    {notesSection}
            
                    <p>Your form has been submitted and is now in the approval process.</p>
            
                    <p>Best regards,<br>The FMST Team</p>
                </div>
        
                <div class='footer'>
                    <p>This email was generated automatically. Please do not reply to this message.</p>
                </div>
            </div>
        </body>
        </html>";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendFormApprovedByAdminEmailAsync(string email, string firstName, string formName, string adminName, DateTime approvedAt, string? notes = null)
        {
            _logger.LogInformation("🔥 SendFormApprovedByAdminEmailAsync called for {Email}, Form: {FormName}, Admin: {AdminName}", 
                email, formName, adminName);
                
            var subject = "Form Approved by Admin - FMST";

            var notesSection = !string.IsNullOrEmpty(notes) 
                ? $@"<p><strong>Admin Notes:</strong></p>
                     <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #007bff; margin: 15px 0;'>
                         {notes}
                     </div>" 
                : "";

            var body = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                .content {{ padding: 20px; }}
                .success-box {{ 
                    background-color: #d4edda; 
                    border: 1px solid #c3e6cb; 
                    padding: 15px; 
                    margin: 20px 0; 
                    border-radius: 5px;
                    border-left: 4px solid #28a745;
                }}
                .footer {{ 
                    background-color: #f8f9fa; 
                    padding: 15px; 
                    text-align: center; 
                    font-size: 12px; 
                    color: #6c757d;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>Form Approved by Admin - FMST</h2>
                </div>
        
                <div class='content'>
                    <p>Hello <strong>{firstName}</strong>,</p>
            
                    <div class='success-box'>
                        <strong>✅ Form approved by admin on your behalf</strong>
                    </div>
            
                    <p><strong>Form Details:</strong></p>
                    <ul>
                        <li><strong>Form:</strong> {formName}</li>
                        <li><strong>Approved by:</strong> {adminName}</li>
                        <li><strong>Approved at:</strong> {approvedAt:dd/MM/yyyy HH:mm}</li>
                    </ul>

                    {notesSection}
            
                    <p>This form has been approved and moved to the next step in the workflow.</p>
            
                    <p>Best regards,<br>The FMST Team</p>
                </div>
        
                <div class='footer'>
                    <p>This email was generated automatically. Please do not reply to this message.</p>
                </div>
            </div>
        </body>
        </html>";

            await SendEmailAsync(email, subject, body, true);
        }

        public async Task SendFormRejectedByAdminEmailAsync(string email, string firstName, string formName, string adminName, DateTime rejectedAt, string rejectReason, string? notes = null)
        {
            _logger.LogInformation("🔥 SendFormRejectedByAdminEmailAsync called for {Email}, Form: {FormName}, Admin: {AdminName}, Reason: {RejectReason}", 
                email, formName, adminName, rejectReason);
                
            var subject = "Form Rejected by Admin - FMST";

            var notesSection = !string.IsNullOrEmpty(notes) 
                ? $@"<p><strong>Admin Notes:</strong></p>
                     <div style='background-color: #f8f9fa; padding: 15px; border-left: 4px solid #007bff; margin: 15px 0;'>
                         {notes}
                     </div>" 
                : "";

            var body = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                .content {{ padding: 20px; }}
                .reject-box {{ 
                    background-color: #f8d7da; 
                    border: 1px solid #f5c6cb; 
                    padding: 15px; 
                    margin: 20px 0; 
                    border-radius: 5px;
                    border-left: 4px solid #dc3545;
                }}
                .reason-box {{
                    background-color: #fff3cd;
                    border: 1px solid #ffeaa7;
                    padding: 15px;
                    margin: 15px 0;
                    border-radius: 5px;
                    border-left: 4px solid #ffc107;
                }}
                .footer {{ 
                    background-color: #f8f9fa; 
                    padding: 15px; 
                    text-align: center; 
                    font-size: 12px; 
                    color: #6c757d;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h2>Form Rejected by Admin - FMST</h2>
                </div>
        
                <div class='content'>
                    <p>Hello <strong>{firstName}</strong>,</p>
            
                    <div class='reject-box'>
                        <strong>❌ Form rejected by admin on your behalf</strong>
                    </div>
            
                    <p><strong>Form Details:</strong></p>
                    <ul>
                        <li><strong>Form:</strong> {formName}</li>
                        <li><strong>Rejected by:</strong> {adminName}</li>
                        <li><strong>Rejected at:</strong> {rejectedAt:dd/MM/yyyy HH:mm}</li>
                    </ul>

                    <div class='reason-box'>
                        <strong>Rejection Reason:</strong>
                        <p>{rejectReason}</p>
                    </div>

                    {notesSection}
            
                    <p>You have been notified that your form was rejected. Please review and take appropriate action.</p>
            
                    <p>Best regards,<br>The FMST Team</p>
                </div>
        
                <div class='footer'>
                    <p>This email was generated automatically. Please do not reply to this message.</p>
                </div>
            </div>
        </body>
        </html>";

            await SendEmailAsync(email, subject, body, true);
        }
    }
}