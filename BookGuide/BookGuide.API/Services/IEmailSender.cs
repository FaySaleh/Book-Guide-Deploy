using BookGuide.API.Services;

using System.Net;
using System.Net.Mail;

namespace BookGuide.API.Services
{
    public interface IEmailSender
    {
        Task SendAsync(string toEmail, string subject, string body);
    }
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var fromEmail = _config["Email:From"];
            var smtpHost = _config["Email:Host"];
            var smtpPort = int.Parse(_config["Email:Port"] ?? "587");
            var smtpUser = _config["Email:Username"];
            var smtpPass = _config["Email:Password"];

            using var message = new MailMessage();
            message.From = new MailAddress(smtpUser!, "Book Guide");
            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using var smtp = new SmtpClient(smtpHost!, smtpPort);
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);

            await smtp.SendMailAsync(message);
        }

    }
}
