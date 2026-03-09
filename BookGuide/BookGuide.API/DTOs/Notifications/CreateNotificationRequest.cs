using System.ComponentModel.DataAnnotations;

namespace BookGuide.API.DTOs.Notifications
{
    public class CreateNotificationRequest
    {
        [Required]
        public int UserId { get; set; }

        public int? UserBookId { get; set; }

        [Required]
        [MaxLength(240)]
        public string Title { get; set; } = null!;

        [Required]
        [MaxLength(600)]
        public string Message { get; set; } = null!;

        [MaxLength(100)]
        public string Type { get; set; } = "General";
    }
}
