using BookGuide.API.Data;
using BookGuide.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BookGuide.API.Services
{
    public class NotificationsService
    {
        private readonly BookGuideDbContext _db;
        private readonly IEmailSender _email;
        private readonly ILogger<NotificationsService> _logger;

        public NotificationsService(
            BookGuideDbContext db,
            IEmailSender email,
            ILogger<NotificationsService> logger)
        {
            _db = db;
            _email = email;
            _logger = logger;
        }

        public async Task CreateAsync(int userId, string title, string message, int? userBookId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                _logger.LogWarning("CreateAsync skipped: user not found. UserId={userId}", userId);
                return;
            }

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

            _logger.LogInformation(
                "Notification created for UserId={userId}, UserBookId={userBookId}",
                userId,
                userBookId);

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                try
                {
                    await _email.SendAsync(user.Email, title, message);
                    _logger.LogInformation(
                        "Reminder email sent successfully to {email} for UserId={userId}",
                        user.Email,
                        userId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "EMAIL ERROR (CreateAsync) for UserId={userId}, Email={email}",
                        userId,
                        user.Email);
                }
            }
            else
            {
                _logger.LogWarning("User has no email. UserId={userId}", userId);
            }
        }

        public async Task<RunRemindersResult> RunReadingRemindersAsync(
            int days = 2,
            CancellationToken ct = default)
        {
            _logger.LogInformation("RunReadingRemindersAsync started. Days={days}", days);

            if (days < 1) days = 1;
            if (days > 30) days = 30;

            var threshold = DateTime.UtcNow.AddDays(-days);
        var cooldown = TimeSpan.FromMinutes(1);
    //  var cooldown = TimeSpan.FromDays(1);
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

            _logger.LogInformation("Candidates found: {count}", candidates.Count);

            var created = 0;

            foreach (var ub in candidates)
            {
                try
                {
                    var alreadySent = await _db.Notifications.AnyAsync(n =>
                        n.UserId == ub.UserId &&
                        n.UserBookId == ub.Id &&
                        n.Type == "ReadingReminder" &&
                        n.CreatedAt >= since,
                        ct);

                    if (alreadySent)
                    {
                        _logger.LogInformation(
                            "Skipped duplicate reminder for UserId={userId}, UserBookId={userBookId}",
                            ub.UserId,
                            ub.Id);

                        continue;
                    }

                    await CreateAsync(
                        userId: ub.UserId,
                        title: "Reading Reminder",
                        message: $"It's been {days} day(s) since you last read \"{ub.Title}\".",
                        userBookId: ub.Id
                    );

                    created++;

                    _logger.LogInformation(
                        "Reminder created for UserId={userId}, UserBookId={userBookId}",
                        ub.UserId,
                        ub.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error while processing reminder for UserId={userId}, UserBookId={userBookId}",
                        ub.UserId,
                        ub.Id);
                }
            }

            _logger.LogInformation(
                "RunReadingRemindersAsync finished. Matched={matched}, Created={created}",
                candidates.Count,
                created);

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