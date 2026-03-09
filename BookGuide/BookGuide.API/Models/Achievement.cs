namespace BookGuide.API.Models
{
    public class Achievement
    {
        public int Id { get; set; }

        public string Code { get; set; } = null!;

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;

        public string? Icon { get; set; }

        public int? TargetValue { get; set; }

        public ICollection<UserAchievement> Users { get; set; } = new List<UserAchievement>();
    }
}
