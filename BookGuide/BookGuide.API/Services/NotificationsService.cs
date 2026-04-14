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
                    var html = @"<!doctype html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <meta name=""x-apple-disable-message-reformatting"" />
  <title>Reading Reminder</title>
</head>
<body style=""margin:0; padding:0; background:#f5f7fb; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Arial, sans-serif; color:#0f172a;"">
  <div style=""display:none; max-height:0; overflow:hidden; opacity:0; color:transparent;"">
    Time to get back to your reading 
  </div>

  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""background:#f5f7fb; padding:24px 12px;"">
    <tr><td align=""center"">
      <table role=""presentation"" width=""600"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""width:600px; max-width:100%;"">

        <!-- Header -->
        <tr>
          <td style=""padding:10px 8px 18px;"">
            <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" border=""0"">
              <tr>
                <td align=""left"" style=""vertical-align:middle;"">
                  <img src=""https://i.ibb.co/TMzSkvpW/Logo.png"" width=""44"" height=""44"" style=""border-radius:10px;"" />
                </td>
                <td align=""left"" style=""padding-left:12px; vertical-align:middle;"">
                  <div style=""font-size:16px; font-weight:800;"">BookGuide</div>
                </td>
              </tr>
            </table>
          </td>
        </tr>

        <!-- Card -->
        <tr>
          <td style=""background:#ffffff; border:1px solid #e6eaf2; border-radius:16px; overflow:hidden; box-shadow:0 10px 24px rgba(15,23,42,0.08);"">
            <table width=""100%"" cellpadding=""0"" cellspacing=""0"">

              <!-- Title -->
              <tr>
                <td style=""padding:26px 22px 10px;"">
                  <h1 style=""margin:0; font-size:22px;"">📚 Reading Reminder</h1>
                  <p style=""margin:12px 0 0; font-size:14px; color:#334155;"">
                    Hi {{USER_NAME}},<br/>
                    It's been a while since you last read:
                  </p>
                </td>
              </tr>

              <!-- Book -->
              <tr>
                <td style=""padding:10px 22px;"">
                  <div style=""font-size:18px; font-weight:700; color:#0f172a;"">
                    {{BOOK_TITLE}}
                  </div>
                </td>
              </tr>

              <!-- Button -->
              <tr>
                <td style=""padding:18px 22px 10px;"">
                  <table>
                    <tr>
                      <td style=""border-radius:12px; background:#4CAF50;"">
                        <a href=""{{APP_URL}}"" style=""display:inline-block; padding:14px 18px; font-size:14px; font-weight:800; color:#ffffff; text-decoration:none; border-radius:12px;"">
                          Continue Reading
                        </a>
                      </td>
                    </tr>
                  </table>

                  <p style=""margin:14px 0 0; font-size:12.5px; color:#64748b;"">
                    Keep your reading streak going 🔥
                  </p>
                </td>
              </tr>

              <tr><td style=""padding:10px 22px;""><div style=""height:1px; background:#eef2f7;""></div></td></tr>

            </table>
          </td>
        </tr>

      </table>
    </td></tr>
  </table>
</body>
</html>";
                    var htmlBody = html
                        .Replace("{{USER_NAME}}", user.FullName)
                        .Replace("{{BOOK_TITLE}}", message)
                        .Replace("{{APP_URL}}", "https://bookguide-ui.onrender.com");

                    await _email.SendAsync(user.Email, "Reading Reminder", htmlBody);
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

            var threshold = DateTime.UtcNow.AddMinutes(-2);
            var cooldown = TimeSpan.FromMinutes(2);
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