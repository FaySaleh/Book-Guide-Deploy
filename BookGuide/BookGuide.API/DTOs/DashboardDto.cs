namespace BookGuide.API.Dtos
{
    public class DashboardDto
    {
        public StatsDto Stats { get; set; } = new();
        public List<AchievementDto> Achievements { get; set; } = new();
    }

    public class StatsDto
    {
        public int TotalBooks { get; set; }
        public int ToRead { get; set; }
        public int Reading { get; set; }
        public int Finished { get; set; }

        public int TotalPagesRead { get; set; }
        public int TotalReadingDays { get; set; }   
        public int CurrentStreakDays { get; set; }  
    }

    public class AchievementDto
    {
        public string Code { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Icon { get; set; }

        public bool Unlocked { get; set; }
        public DateTime? UnlockedAt { get; set; }

        public int? TargetValue { get; set; }
        public int CurrentValue { get; set; }
        public int ProgressPercent { get; set; }
    }
}
