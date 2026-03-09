namespace BookGuide.API.DTOs
{
    public class BookSearchResultDto
    {
        public string ExternalBookId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Author { get; set; }
        public string? CoverUrl { get; set; }
    }
}
