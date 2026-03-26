using BookGuide.API.Data;
using Microsoft.EntityFrameworkCore;

namespace BookGuide.API.Services
{
    public class DashboardService
    {
        private readonly BookGuideDbContext _db;
        private readonly AchievementsService _achievementsService;

        public DashboardService(BookGuideDbContext db, AchievementsService achievementsService)
        {
            _db = db;
            _achievementsService = achievementsService;
        }

        public async Task<object> GetDashboardAsync(int userId)
        {
            await _achievementsService.EvaluateAsync(userId);

            var userBooks = await _db.UserBooks
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var totalBooks = userBooks.Count;
            var toRead = userBooks.Count(x => x.Status == 0);
            var reading = userBooks.Count(x => x.Status == 1);
            var finished = userBooks.Count(x => x.Status == 2);
            var totalPagesRead = userBooks.Sum(x => x.CurrentPage ?? 0);

            var totalReadingDays = userBooks
                .Select(x => (x.LastReadAt ?? x.LastProgressAt)?.Date)
                .Where(x => x != null)
                .Select(x => x!.Value)
                .Distinct()
                .Count();

            var currentStreakDays = _achievementsService.CalculateStreakDays(userBooks);

            var allAchievements = await _db.Achievements.ToListAsync();

            Console.WriteLine("=== DASHBOARD DEBUG ===");
            Console.WriteLine("UserId: " + userId);
            Console.WriteLine("Books count: " + userBooks.Count);
            Console.WriteLine("Achievements in DB: " + allAchievements.Count);

            var unlockedAchievements = await _db.UserAchievements
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var achievements = allAchievements.Select(a =>
            {
                var unlocked = unlockedAchievements.FirstOrDefault(x => x.AchievementId == a.Id);

                int currentValue = a.Code switch
                {
                    "FIRST_BOOK" => finished,
                    "FIVE_BOOKS" => finished,
                    "TEN_BOOKS" => finished,
                    "PAGES_1000" => totalPagesRead,
                    "STREAK_7" => currentStreakDays,
                    _ => 0
                };

                int targetValue = a.TargetValue ?? 1;
                if (targetValue <= 0) targetValue = 1;

                int progressPercent = Math.Min(100,
                    (int)Math.Round((double)currentValue / targetValue * 100));

                return new
                {
                    code = a.Code,
                    title = a.Title,
                    description = a.Description,
                    icon = a.Icon,
                    unlocked = unlocked != null,
                    unlockedAt = unlocked?.UnlockedAt,
                    targetValue = targetValue,
                    currentValue = currentValue,
                    progressPercent = progressPercent
                };
            }).ToList();

            Console.WriteLine("Returned achievements count: " + achievements.Count);

            return new
            {
                stats = new
                {
                    totalBooks,
                    toRead,
                    reading,
                    finished,
                    totalPagesRead,
                    totalReadingDays,
                    currentStreakDays
                },
                achievements
            };
        }
    }
}