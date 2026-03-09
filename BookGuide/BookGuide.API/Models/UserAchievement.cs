namespace BookGuide.API.Models
{
    public class UserAchievement
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int AchievementId { get; set; }

        public DateTime UnlockedAt { get; set; }

        public User User { get; set; } = null!;
        public Achievement Achievement { get; set; } = null!;
    }
}
