namespace BookGuide.API.Models
{
    public class UserBook
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string ExternalBookId { get; set; } = null!;
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? CoverUrl { get; set; }

        public int Status { get; set; } 
        public int? Rating { get; set; }
        public DateTime CreatedAt { get; set; }

        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int? CurrentPage { get; set; }
        public int? TotalPages { get; set; }
        public DateTime? LastProgressAt { get; set; }
        public DateTime? LastReadAt { get; set; }
        public User User { get; set; } = null!;
    }
}
