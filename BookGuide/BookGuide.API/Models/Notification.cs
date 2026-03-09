using System.ComponentModel.DataAnnotations;

namespace BookGuide.API.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int? UserBookId { get; set; }

        [MaxLength(240)]
        public string Title { get; set; } = null!;

        [MaxLength(600)]
        public string Message { get; set; } = null!;

        [MaxLength(100)]
        public string Type { get; set; } = "ReadingReminder";

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public UserBook? UserBook { get; set; }
    }
}
