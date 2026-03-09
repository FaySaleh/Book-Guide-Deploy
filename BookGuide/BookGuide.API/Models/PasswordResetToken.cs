namespace BookGuide.API.Models
{
    public class PasswordResetToken
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string TokenHash { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiryAt { get; set; }

        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}
