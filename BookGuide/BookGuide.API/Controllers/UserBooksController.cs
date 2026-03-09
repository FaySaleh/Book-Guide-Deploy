using BookGuide.API.Data;
using BookGuide.API.DTOs;
using BookGuide.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookGuide.API.Services;

namespace BookGuide.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserBooksController : ControllerBase
    {
        private readonly BookGuideDbContext _db;

        public UserBooksController(BookGuideDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserBooks([FromQuery] int userId, [FromQuery] int? status)
        {
            if (userId <= 0)
                return BadRequest("userId is required.");

            var query = _db.UserBooks.AsNoTracking().Where(x => x.UserId == userId);

            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);

            var list = await query
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.UserId,
                    x.ExternalBookId,
                    x.Title,
                    x.Author,
                    x.CoverUrl,
                    x.Status,
                    x.Rating,
                    x.CreatedAt,

                    x.StartedAt,
                    x.FinishedAt,
                    x.CurrentPage,
                    x.TotalPages,
                    x.LastProgressAt,

                    x.LastReadAt
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddUserBookRequest request)
        {
            if (request.UserId <= 0 || string.IsNullOrWhiteSpace(request.ExternalBookId))
                return BadRequest("UserId and ExternalBookId are required.");

            var userExists = await _db.Users.AnyAsync(u => u.Id == request.UserId);
            if (!userExists)
                return NotFound("User not found.");

            var already = await _db.UserBooks.AnyAsync(x =>
                x.UserId == request.UserId && x.ExternalBookId == request.ExternalBookId);

            if (already)
                return Conflict("This book already exists for this user.");

            if (request.Rating.HasValue && (request.Rating < 1 || request.Rating > 5))
                return BadRequest("Rating must be between 1 and 5.");

            if (request.Status < 1 || request.Status > 3)
                return BadRequest("Status must be 1(ToRead), 2(Reading), 3(Finished).");

            var now = DateTime.UtcNow;

            var entity = new UserBook
            {
                UserId = request.UserId,
                ExternalBookId = request.ExternalBookId.Trim(),
                Title = request.Title,
                Author = request.Author,
                CoverUrl = request.CoverUrl,
                Status = request.Status,
                Rating = request.Rating,
                CreatedAt = now,

                StartedAt = request.Status == 2 ? now : null,
                FinishedAt = request.Status == 3 ? now : null,
                CurrentPage = null,
                TotalPages = null,
                LastProgressAt = null,

                LastReadAt = request.Status == 2 ? now : null
            };


            _db.UserBooks.Add(entity);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                entity.Id,
                entity.UserId,
                entity.ExternalBookId,
                entity.Title,
                entity.Author,
                entity.CoverUrl,
                entity.Status,
                entity.Rating,
                entity.CreatedAt,

                entity.StartedAt,
                entity.FinishedAt,
                entity.CurrentPage,
                entity.TotalPages,
                entity.LastProgressAt,

                entity.LastReadAt
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateUserBookRequest request)
        {
            if (request.Rating.HasValue && (request.Rating < 1 || request.Rating > 5))
                return BadRequest("Rating must be between 1 and 5.");

            if (request.Status < 1 || request.Status > 3)
                return BadRequest("Status must be 1(ToRead), 2(Reading), 3(Finished).");

            var entity = await _db.UserBooks.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound("UserBook not found.");

            var now = DateTime.UtcNow;

            if (request.Status == 2 && entity.StartedAt == null)
                entity.StartedAt = now;

            if (request.Status == 2)
                entity.LastReadAt = now;

            if (request.Status == 3)
            {
                entity.StartedAt ??= now;
                entity.FinishedAt ??= now;

                if (entity.TotalPages.HasValue && !entity.CurrentPage.HasValue)
                    entity.CurrentPage = entity.TotalPages;

                entity.LastReadAt = now;
            }

            entity.Status = request.Status;
            entity.Rating = request.Rating;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id:int}/progress")]
        public async Task<IActionResult> GetProgress(int id)
        {
            var entity = await _db.UserBooks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound("UserBook not found.");

            return Ok(new
            {
                entity.Id,
                entity.UserId,
                entity.Title,
                entity.Status,

                entity.StartedAt,
                entity.FinishedAt,
                entity.CurrentPage,
                entity.TotalPages,
                entity.LastProgressAt,

                entity.LastReadAt
            });
        }


        [HttpPut("{id:int}/progress")]
        public async Task<IActionResult> UpdateProgress(int id, UpdateProgressRequest request)
        {
            var entity = await _db.UserBooks.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound("UserBook not found.");

            if (request.CurrentPage.HasValue && request.CurrentPage.Value < 0)
                return BadRequest("CurrentPage must be >= 0.");

            if (request.TotalPages.HasValue && request.TotalPages.Value <= 0)
                return BadRequest("TotalPages must be > 0.");

            if (request.CurrentPage.HasValue && request.TotalPages.HasValue &&
                request.CurrentPage.Value > request.TotalPages.Value)
                return BadRequest("CurrentPage cannot be greater than TotalPages.");

            var now = DateTime.UtcNow;

            bool changedProgress = false;

            if (request.TotalPages.HasValue)
            {
                entity.TotalPages = request.TotalPages.Value;
                changedProgress = true;
            }

            if (request.CurrentPage.HasValue)
            {
                entity.CurrentPage = request.CurrentPage.Value;
                changedProgress = true;

                entity.LastReadAt = now;
            }

            if (changedProgress)
                entity.LastProgressAt = now;

            if (entity.CurrentPage.HasValue && entity.CurrentPage.Value > 0)
            {
                entity.StartedAt ??= now;

                if (entity.Status == 1) 
                    entity.Status = 2; 
            }

            if (entity.TotalPages.HasValue && entity.TotalPages.Value > 0 &&
                entity.CurrentPage.HasValue &&
                entity.CurrentPage.Value >= entity.TotalPages.Value)
            {
                entity.Status = 3; 
                entity.StartedAt ??= now;
                entity.FinishedAt ??= now;

                entity.CurrentPage = entity.TotalPages.Value;

                entity.LastReadAt = now;
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                entity.Id,
                entity.UserId,
                entity.Title,
                entity.Status,

                entity.StartedAt,
                entity.FinishedAt,
                entity.CurrentPage,
                entity.TotalPages,
                entity.LastProgressAt,

                entity.LastReadAt
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.UserBooks.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound("UserBook not found.");

            _db.UserBooks.Remove(entity);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        public class UpdateProgressRequest
        {
            public int? CurrentPage { get; set; }
            public int? TotalPages { get; set; }
        }
    }
}
