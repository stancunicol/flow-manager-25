using FlowManager.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml = false)
    {
        var smtpHost = _config["SmtpSettings:Host"];
        var smtpPort = int.Parse(_config["SmtpSettings:Port"]);
        var smtpUser = _config["SmtpSettings:Username"];
        var smtpPass = _config["SmtpSettings:Password"];
        var from = _config["SmtpSettings:FromEmail"];

        var mail = new MailMessage(from, to)
        {
            Subject = subject,
            Body = body,
            IsBodyHtml = isBodyHtml
        };

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true
        };
        await client.SendMailAsync(mail);
    }
}