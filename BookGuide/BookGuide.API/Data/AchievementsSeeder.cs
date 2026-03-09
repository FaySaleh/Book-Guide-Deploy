using BookGuide.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BookGuide.API.Data
{
    public static class AchievementsSeeder
    {
        public static async Task SeedAsync(BookGuideDbContext db)
        {
            var items = new List<Achievement>
            {
                new() { Code = "FIRST_BOOK", Title = "First Book", Description = "Finish your first book", Icon = "📘", TargetValue = 1 },
                new() { Code = "FIVE_BOOKS", Title = "5 Books", Description = "Finish 5 books", Icon = "📚", TargetValue = 5 },
                new() { Code = "TEN_BOOKS", Title = "10 Books", Description = "Finish 10 books", Icon = "🏆", TargetValue = 10 },
                new() { Code = "PAGES_1000", Title = "1000 Pages", Description = "Read 1000 pages", Icon = "💯", TargetValue = 1000 },
                new() { Code = "STREAK_7", Title = "7-day Streak", Description = "Read 7 days in a row", Icon = "🔥", TargetValue = 7 },
            };

            var existingCodes = await db.Achievements
                .Select(a => a.Code)
                .ToListAsync();

            var missing = items.Where(x => !existingCodes.Contains(x.Code)).ToList();

            if (missing.Count == 0) return;

            db.Achievements.AddRange(missing);
            await db.SaveChangesAsync();
        }
    }
}
