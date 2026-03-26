using BookGuide.API.Data;
using Microsoft.EntityFrameworkCore;

namespace BookGuide.API.Services
{
    public class DashboardService
    {
        private readonly BookGuideDbContext _db;

        public DashboardService(BookGuideDbContext db)
        {
            _db = db;
        }

        public async Task<object> GetDashboardAsync(int userId)
        {
            var userBooks = await _db.UserBooks
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var totalBooks = userBooks.Count;
            var toRead = userBooks.Count(x => x.Status == 0);
            var reading = userBooks.Count(x => x.Status == 1);
            var finished = userBooks.Count(x => x.Status == 2);

            var totalPagesRead = userBooks.Sum(x => x.CurrentPage ?? 0);

            return new
            {
                stats = new
                {
                    totalBooks,
                    toRead,
                    reading,
                    finished,
                    totalPagesRead,
                    totalReadingDays = 0,
                    currentStreakDays = 0
                },
                achievements = new List<object>()
            };
        }
    }
}