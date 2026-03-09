namespace BookGuide.API.DTOs
{
    public class UpdateReadingProgressRequest
    {
        public int? CurrentPage { get; set; }
        public int? TotalPages { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }
}