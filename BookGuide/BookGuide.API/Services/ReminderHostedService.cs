using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookGuide.API.Services
{
    public class ReminderHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReminderHostedService> _logger;

        public ReminderHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<ReminderHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ReminderHostedService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var notificationsService = scope.ServiceProvider.GetRequiredService<NotificationsService>();

                    _logger.LogInformation("Running reading reminders at: {time}", DateTime.UtcNow);

                    var result = await notificationsService.RunReadingRemindersAsync(2, stoppingToken);

                    _logger.LogInformation(
                        "Reading reminders completed. Matched={matched}, Created={created}",
                        result.Matched,
                        result.Created);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("ReminderHostedService is stopping.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while running ReminderHostedService.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("ReminderHostedService delay cancelled.");
                    break;
                }
            }

            _logger.LogInformation("ReminderHostedService stopped.");
        }
    }
}