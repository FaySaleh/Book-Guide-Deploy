namespace BookGuide.API.DTOs
{
    public class AddUserBookRequest
    {
        public int UserId { get; set; }
        public string ExternalBookId { get; set; } = null!;
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? CoverUrl { get; set; }
        public int Status { get; set; } = 1; 
        public int? Rating { get; set; } 
    }
}
