using System.Net;
using System.Net.Mail;

namespace BookGuide.API.Services
{
    public interface IEmailSender
    {
        Task SendAsync(string toEmail, string subject, string html);
    }

    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var fromEmail = _config["Email:From"];
            var smtpHost = _config["Email:Host"];
            var smtpPortValue = _config["Email:Port"];
            var smtpUser = _config["Email:Username"];
            var smtpPass = _config["Email:Password"];

            _logger.LogInformation("EMAIL DEBUG: Host={Host}", smtpHost);
            _logger.LogInformation("EMAIL DEBUG: Port={Port}", smtpPortValue);
            _logger.LogInformation("EMAIL DEBUG: Username={Username}", smtpUser);
            _logger.LogInformation("EMAIL DEBUG: From={From}", fromEmail);
            _logger.LogInformation("EMAIL DEBUG: To={To}", toEmail);

            if (string.IsNullOrWhiteSpace(fromEmail))
                throw new Exception("Email:From is missing.");

            if (string.IsNullOrWhiteSpace(smtpHost))
                throw new Exception("Email:Host is missing.");

            if (string.IsNullOrWhiteSpace(smtpUser))
                throw new Exception("Email:Username is missing.");

            if (string.IsNullOrWhiteSpace(smtpPass))
                throw new Exception("Email:Password is missing.");

            if (!int.TryParse(smtpPortValue, out var smtpPort))
                smtpPort = 587;

            using var message = new MailMessage();
            message.From = new MailAddress(fromEmail, "Book Guide");
            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using var smtp = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000
            };

            try
            {
                _logger.LogInformation("EMAIL DEBUG: Starting SMTP send...");
                await smtp.SendMailAsync(message);
                _logger.LogInformation("EMAIL DEBUG: Email sent successfully to {To}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EMAIL ERROR: Failed to send email to {To}. Message: {Message}", toEmail, ex.Message);

                if (ex.InnerException != null)
                {
                    _logger.LogError("EMAIL ERROR INNER: {InnerMessage}", ex.InnerException.Message);
                }

                throw;
            }
        }
    }
}