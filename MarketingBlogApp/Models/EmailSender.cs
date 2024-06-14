using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

public class EmailSender : IEmailSender
{
    public class EmailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
    }

    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<EmailSettings> emailSettings, ILogger<EmailSender> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        try
        {
            var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.From),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            _logger.LogInformation("Sending email to {0}", email);
            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to: {0}", email);
        }
        catch (SmtpException smtpEx)
        {
            _logger.LogError("SMTP Error: {0}", smtpEx.Message);
            throw; // Re-throw to ensure any calling method is aware of the failure
        }
        catch (Exception ex)
        {
            _logger.LogError("Error sending email: {0}", ex.Message);
            throw; // Re-throw to ensure any calling method is aware of the failure
        }
    }
}
