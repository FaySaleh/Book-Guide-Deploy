using BookGuide.API.Data;
using BookGuide.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BookGuide.API.Services
{
    public class AchievementsService
    {
        private readonly BookGuideDbContext _db;

        public AchievementsService(BookGuideDbContext db)
        {
            _db = db;
        }

        public async Task EvaluateAsync(int userId)
        {
            var books = await _db.UserBooks
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var finishedCount = books.Count(x => x.Status == 2 /* Finished */);
            var pagesRead = books.Sum(x => x.CurrentPage ?? 0);

            var streak = CalculateStreakDays(books);

            var achievements = await _db.Achievements.AsNoTracking().ToListAsync();

            var unlockedIds = await _db.UserAchievements
                .Where(x => x.UserId == userId)
                .Select(x => x.AchievementId)
                .ToListAsync();

            var toUnlock = new List<int>();

            foreach (var a in achievements)
            {
                if (unlockedIds.Contains(a.Id)) continue;

                var ok = a.Code switch
                {
                    "FIRST_BOOK" => finishedCount >= 1,
                    "FIVE_BOOKS" => finishedCount >= 5,
                    "TEN_BOOKS" => finishedCount >= 10,
                    "PAGES_1000" => pagesRead >= 1000,
                    "STREAK_7" => streak >= 7,
                    _ => false
                };

                if (ok) toUnlock.Add(a.Id);
            }

            if (toUnlock.Count == 0) return;

            var now = DateTime.UtcNow;

            foreach (var achievementId in toUnlock)
            {
                _db.UserAchievements.Add(new UserAchievement
                {
                    UserId = userId,
                    AchievementId = achievementId,
                    UnlockedAt = now
                });
            }

            await _db.SaveChangesAsync();
        }

        public int CalculateStreakDays(List<UserBook> books)
        {
            var days = books
                .Select(b => (b.LastReadAt ?? b.LastProgressAt)?.Date)
                .Where(d => d != null)
                .Select(d => d!.Value)
                .Distinct()
                .ToHashSet();

            if (days.Count == 0) return 0;

            var streak = 0;
            var cursor = DateTime.UtcNow.Date;

            while (days.Contains(cursor))
            {
                streak++;
                cursor = cursor.AddDays(-1);
            }

            return streak;
        }
    }
}
