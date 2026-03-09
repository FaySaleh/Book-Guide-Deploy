using BookGuide.API.Data;
using BookGuide.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BookGuide.API.Services
{
    public class NotificationsService
    {
        private readonly BookGuideDbContext _db;
        private readonly IEmailSender _email;

        public NotificationsService(BookGuideDbContext db, IEmailSender email)
        {
            _db = db;
            _email = email;
        }

        public async Task CreateAsync(int userId, string title, string message, int? userBookId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return;

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                UserBookId = userBookId,
                Type = "ReadingReminder",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                try
                {
                    await _email.SendAsync(user.Email, title, message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EMAIL ERROR (CreateAsync): " + ex);
                }
            }
        }

        public async Task<RunRemindersResult> RunReadingRemindersAsync(
            int days = 2,
            CancellationToken ct = default)
        {
            if (days < 1) days = 1;
            if (days > 30) days = 30;

            var threshold = DateTime.UtcNow.AddDays(-days);

            var cooldown = TimeSpan.FromDays(1);
            var since = DateTime.UtcNow - cooldown;

            var candidates = await _db.UserBooks
                .Where(ub =>
                    ub.Status == (int)ReadingStatus.Reading &&
                    ub.StartedAt != null &&
                    (ub.LastReadAt ?? ub.LastProgressAt ?? ub.StartedAt) < threshold
                )
                .Select(ub => new Candidate
                {
                    Id = ub.Id,
                    UserId = ub.UserId,
                    Title = ub.Title,
                    Last = (ub.LastReadAt ?? ub.LastProgressAt ?? ub.StartedAt)
                })
                .ToListAsync(ct);

            var created = 0;

            foreach (var ub in candidates)
            {
                var alreadySent = await _db.Notifications.AnyAsync(n =>
                    n.UserId == ub.UserId &&
                    n.UserBookId == ub.Id &&
                    n.Type == "ReadingReminder" &&
                    n.CreatedAt >= since,
                    ct);

                if (alreadySent) continue;

                await CreateAsync(
                    userId: ub.UserId,
                    title: "Reading Reminder",
                    message: $"it's been {days} day(s) since you last read \"{ub.Title}\"",
                    userBookId: ub.Id
                );

                created++;
            }

            return new RunRemindersResult
            {
                Days = days,
                Threshold = threshold,
                Matched = candidates.Count,
                Created = created,
                Sample = candidates.Take(5).Select(x => new CandidateSample
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Title = x.Title,
                    Last = x.Last
                }).ToList()
            };
        }
    }

    public class RunRemindersResult
    {
        public int Days { get; set; }
        public DateTime Threshold { get; set; }
        public int Matched { get; set; }
        public int Created { get; set; }
        public List<CandidateSample> Sample { get; set; } = new();
    }

    public class Candidate
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Title { get; set; }
        public DateTime? Last { get; set; }
    }

    public class CandidateSample
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Title { get; set; }
        public DateTime? Last { get; set; }
    }
}
