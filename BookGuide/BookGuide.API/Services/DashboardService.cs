using BookGuide.API.Data;
using BookGuide.API.Dtos;
using BookGuide.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BookGuide.API.Services
{
    public class DashboardService
    {
        private readonly BookGuideDbContext _db;
        private readonly AchievementsService _achievements;

        public DashboardService(BookGuideDbContext db, AchievementsService achievements)
        {
            _db = db;
            _achievements = achievements;
        }

        public async Task<DashboardDto> GetAsync(int userId)
        {
            await _achievements.EvaluateAsync(userId);

            var books = await _db.UserBooks
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var total = books.Count;
            var toRead = books.Count(x => x.Status == (int)ReadingStatus.ToRead);     
            var reading = books.Count(x => x.Status == (int)ReadingStatus.Reading);   
            var finished = books.Count(x => x.Status == (int)ReadingStatus.Finished); 

            var pagesRead = books.Sum(x => x.CurrentPage ?? 0);

            var totalReadingDays = books.Sum(b => GetReadingDays(b) ?? 0);

            var streak = _achievements.CalculateStreakDays(books);

            var allAchievements = await _db.Achievements.AsNoTracking().ToListAsync();

            var unlocked = await _db.UserAchievements
                .Include(x => x.Achievement)
                .Where(x => x.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            var unlockedByCode = unlocked.ToDictionary(x => x.Achievement.Code, x => x);

            var result = new DashboardDto
            {
                Stats = new StatsDto
                {
                    TotalBooks = total,
                    ToRead = toRead,
                    Reading = reading,
                    Finished = finished,
                    TotalPagesRead = pagesRead,
                    TotalReadingDays = totalReadingDays,
                    CurrentStreakDays = streak
                }
            };

            foreach (var a in allAchievements)
            {
                var (currentValue, percent) = a.Code switch
                {
                    "FIRST_BOOK" => Progress(finished, a.TargetValue ?? 1),
                    "FIVE_BOOKS" => Progress(finished, a.TargetValue ?? 5),
                    "TEN_BOOKS" => Progress(finished, a.TargetValue ?? 10),
                    "PAGES_1000" => Progress(pagesRead, a.TargetValue ?? 1000),
                    "STREAK_7" => Progress(streak, a.TargetValue ?? 7),
                    _ => (0, 0)
                };

                var isUnlocked = unlockedByCode.TryGetValue(a.Code, out var ua);

                result.Achievements.Add(new AchievementDto
                {
                    Code = a.Code,
                    Title = a.Title,
                    Description = a.Description,
                    Icon = a.Icon,
                    TargetValue = a.TargetValue,
                    CurrentValue = currentValue,
                    ProgressPercent = percent,
                    Unlocked = isUnlocked,
                    UnlockedAt = ua?.UnlockedAt
                });
            }

            result.Achievements = result.Achievements
                .OrderByDescending(x => x.Unlocked)
                .ThenByDescending(x => x.ProgressPercent)
                .ToList();

            return result;
        }

        private static (int current, int percent) Progress(int current, int target)
        {
            if (target <= 0) return (current, 0);
            var p = (int)Math.Round((double)current * 100.0 / target);
            p = Math.Clamp(p, 0, 100);
            return (current, p);
        }

        private static int? GetReadingDays(Models.UserBook b)
        {
            if (b.StartedAt == null) return null;

            var start = b.StartedAt.Value.Date;
            var end = (b.FinishedAt ?? DateTime.UtcNow).Date;

            var days = (end - start).Days + 1;
            return Math.Max(days, 1);
        }
    }
}
