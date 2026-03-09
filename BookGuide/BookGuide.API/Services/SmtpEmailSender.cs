using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace BookGuide.API.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public SmtpEmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var host = _config["Email:Host"];
            var portStr = _config["Email:Port"];
            var username = _config["Email:Username"];
            var password = _config["Email:Password"];
            var from = _config["Email:From"];

            if (string.IsNullOrWhiteSpace(host))
                throw new InvalidOperationException("Email:Host is missing in appsettings.json");

            if (!int.TryParse(portStr, out var port))
                throw new InvalidOperationException("Email:Port is invalid in appsettings.json");

            if (string.IsNullOrWhiteSpace(username))
                throw new InvalidOperationException("Email:Username is missing in appsettings.json");

            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Email:Password is missing in appsettings.json");

            if (string.IsNullOrWhiteSpace(from))
                throw new InvalidOperationException("Email:From is missing in appsettings.json");

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, password),
                Timeout = 20000
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(from, "BookGuide"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true   // 🔥🔥 هذا هو الحل
            };

            mail.To.Add(toEmail);

            try
            {
                Console.WriteLine("EMAIL: starting send...");
                await client.SendMailAsync(mail);
                Console.WriteLine("EMAIL: sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine("EMAIL ERROR (SmtpEmailSender): " + ex.ToString());
                throw;
            }
        }
    }
}
