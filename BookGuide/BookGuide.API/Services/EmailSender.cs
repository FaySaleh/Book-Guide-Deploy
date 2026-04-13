using System.Net;
using System.Net.Mail;

namespace BookGuide.API.Services
{
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

            _logger.LogInformation("Email send started. To={ToEmail}, Host={Host}, Port={Port}, From={From}",
                toEmail, smtpHost, smtpPortValue, fromEmail);
            _logger.LogInformation("SMTP username configured: {User}", smtpUser);

            if (string.IsNullOrWhiteSpace(fromEmail) ||
                string.IsNullOrWhiteSpace(smtpHost) ||
                string.IsNullOrWhiteSpace(smtpUser) ||
                string.IsNullOrWhiteSpace(smtpPass))
            {
                throw new Exception("Email configuration is missing.");
            }

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
                Timeout = 10000
            };

            try
            {
                await smtp.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email sending failed to {ToEmail}", toEmail);
                throw;
            }
        }
    }
}