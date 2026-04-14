using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BookGuide.API.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailSender> _logger;
        private readonly HttpClient _httpClient;

        public EmailSender(
            IConfiguration config,
            ILogger<EmailSender> logger,
            IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task SendAsync(string toEmail, string subject, string html)
        {
            var apiKey = _config["Resend:ApiKey"];
            var fromEmail = _config["Resend:FromEmail"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Resend API key is missing.");

            if (string.IsNullOrWhiteSpace(fromEmail))
                throw new Exception("Resend FromEmail is missing.");

            _logger.LogInformation(
                "Resend send started. To={ToEmail}, From={FromEmail}",
                toEmail, fromEmail);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.resend.com/emails");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                from = fromEmail,
                to = new[] { toEmail },
                subject = subject,
                html = html
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            using var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Resend send failed. Status={StatusCode}, Body={Body}",
                    response.StatusCode, responseBody);

                throw new Exception("Resend email failed: " + responseBody);
            }

            _logger.LogInformation(
                "Resend email sent successfully to {ToEmail}",
                toEmail);
        }
    }
}