using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BookGuide.API.Services
{
    public class ResendEmailSender : IEmailSender
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<ResendEmailSender> _logger;

        public ResendEmailSender(
            HttpClient http,
            IConfiguration config,
            ILogger<ResendEmailSender> logger)
        {
            _http = http;
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(string toEmail, string subject, string html)
        {
            var apiKey = _config["RESEND_API_KEY"];
            var from = _config["RESEND_FROM"];

            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var body = new
            {
                from = from,
                to = new[] { toEmail },
                subject = subject,
                html = html
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.PostAsync("https://api.resend.com/emails", content);
            var result = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Resend response: {Result}", result);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Resend failed: {result}");
        }
    }
}