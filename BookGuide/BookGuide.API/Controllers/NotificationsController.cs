using BookGuide.API.Data;
using BookGuide.API.DTOs.Notifications;
using BookGuide.API.Models;
using BookGuide.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookGuide.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly BookGuideDbContext _db;
        private readonly NotificationsService _notificationsService;

        public NotificationsController(BookGuideDbContext db, NotificationsService notificationsService)
        {
            _db = db;
            _notificationsService = notificationsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool onlyUnread = false)
        {
            if (userId <= 0) return BadRequest("userId is required.");
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var q = _db.Notifications.AsNoTracking().Where(n => n.UserId == userId);

            if (onlyUnread)
                q = q.Where(n => !n.IsRead);

            var total = await q.CountAsync();

            var items = await q.OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    UserBookId = n.UserBookId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                total,
                items
            });
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount([FromQuery] int userId)
        {
            if (userId <= 0) return BadRequest("userId is required.");

            var count = await _db.Notifications
                .AsNoTracking()
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return Ok(new { userId, unread = count });
        }

        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id, [FromQuery] int userId)
        {
            if (userId <= 0) return BadRequest("userId is required.");

            var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (n == null) return NotFound("Notification not found.");

            if (!n.IsRead)
            {
                n.IsRead = true;
                await _db.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllRead([FromQuery] int userId)
        {
            if (userId <= 0) return BadRequest("userId is required.");

            var list = await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (list.Count == 0)
                return Ok(new { updated = 0 });

            foreach (var n in list)
                n.IsRead = true;

            await _db.SaveChangesAsync();
            return Ok(new { updated = list.Count });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateNotificationRequest req)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userExists = await _db.Users.AnyAsync(u => u.Id == req.UserId);
            if (!userExists)
                return NotFound("User not found.");

            if (req.UserBookId.HasValue)
            {
                var ubExists = await _db.UserBooks.AnyAsync(ub =>
                    ub.Id == req.UserBookId.Value && ub.UserId == req.UserId);

                if (!ubExists)
                    return BadRequest("UserBookId is invalid for this user.");
            }

            var n = new Notification
            {
                UserId = req.UserId,
                UserBookId = req.UserBookId,
                Title = req.Title.Trim(),
                Message = req.Message.Trim(),
                Type = string.IsNullOrWhiteSpace(req.Type) ? "General" : req.Type.Trim(),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _db.Notifications.Add(n);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                n.Id,
                n.UserId,
                n.UserBookId,
                n.Title,
                n.Message,
                n.Type,
                n.IsRead,
                n.CreatedAt
            });
        }

        [HttpPost("run-reminders")]
        public async Task<IActionResult> RunReminders([FromQuery] int days = 2)
        {
            var result = await _notificationsService.RunReadingRemindersAsync(days);
            return Ok(result);
        }

        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail([FromQuery] int userId = 2)
        {
            await _notificationsService.CreateAsync(
                userId: userId,
                title: "Test Email",
                message: "إذا وصل هذا الإيميل فـ SMTP مضبوط ✅",
                userBookId: null
            );

            return Ok("Triggered CreateAsync (DB + Email)");
        }



    }
}
