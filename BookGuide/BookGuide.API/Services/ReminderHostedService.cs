using BookGuide.API.Data;
using BookGuide.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BookGuide.API.Services
{
    public class ReminderHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ReminderHostedService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine($"[REMINDER] Running job at UTC: {DateTime.UtcNow:O}");
                    await Run(stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("REMINDER JOB ERROR: " + ex);
                }

                var saudiOffset = TimeSpan.FromHours(3);

                var nowUtc = DateTimeOffset.UtcNow;
                var nowSaudi = nowUtc.ToOffset(saudiOffset);

                var nextRunSaudi = new DateTimeOffset(
                    nowSaudi.Year,
                    nowSaudi.Month,
                    nowSaudi.Day,
                    10, 0, 0,
                    saudiOffset);

                if (nowSaudi >= nextRunSaudi)
                {
                    nextRunSaudi = nextRunSaudi.AddDays(1);
                }

                var nextRunUtc = nextRunSaudi.ToUniversalTime();

                var delay = nextRunUtc - nowUtc;


                Console.WriteLine($"[REMINDER] Next run (Saudi): {nextRunSaudi:yyyy-MM-dd HH:mm} | delay: {delay}");

                await Task.Delay(delay, stoppingToken);
            }
        }

        private async Task Run(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<BookGuideDbContext>();
            var notifications = scope.ServiceProvider.GetRequiredService<NotificationsService>();

            const int days = 1;
            var threshold = DateTime.UtcNow.AddDays(-days);

            var cooldown = TimeSpan.FromDays(1);
            var since = DateTime.UtcNow - cooldown;

            var books = await db.UserBooks
                .Where(ub =>
                    ub.Status == (int)ReadingStatus.Reading &&
                    ub.StartedAt != null &&
                    (ub.LastReadAt ?? ub.LastProgressAt ?? ub.StartedAt) < threshold
                )
                .Select(ub => new { ub.Id, ub.UserId, ub.Title })
                .ToListAsync(ct);

            foreach (var ub in books)
            {
                var alreadySent = await db.Notifications.AnyAsync(n =>
                    n.UserId == ub.UserId &&
                    n.UserBookId == ub.Id &&
                    n.Type == "ReadingReminder" &&
                    n.CreatedAt >= since,
                    ct);

                if (alreadySent) continue;

                await notifications.CreateAsync(
                    userId: ub.UserId,
                    title: "Reading Reminder",
                    message: $"it's been {days} day(s) since you last read \"{ub.Title}\"",
                    userBookId: ub.Id
                );
            }
        }
    }
}
