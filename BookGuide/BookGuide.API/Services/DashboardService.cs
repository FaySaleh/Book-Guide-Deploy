using BookGuide.API.Data;
using BookGuide.API.Models;
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

        public async Task<object> GetAsync(int userId)
        {
            var books = await _db.UserBooks
                .Where(x => x.UserId == userId)
                .ToListAsync();

            var totalBooks = books.Count;

            var toRead = books.Count(x => x.Status == (int)ReadingStatus.ToRead);
            var reading = books.Count(x => x.Status == (int)ReadingStatus.Reading);
            var finished = books.Count(x => x.Status == (int)ReadingStatus.Finished);

            var totalPagesRead = books.Sum(x => x.CurrentPage ?? 0);

            var totalReadingDays = books
                .Where(x => x.StartedAt != null)
                .Count();

            var currentStreakDays = 0;

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
                achievements = new List<object>()
            };
        }
    }
}