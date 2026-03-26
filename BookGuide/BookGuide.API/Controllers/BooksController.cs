using BookGuide.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BookGuide.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BooksController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string title)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(title))
                    return BadRequest("title is required.");

                var client = _httpClientFactory.CreateClient();

                var url = $"https://openlibrary.org/search.json?title={Uri.EscapeDataString(title)}";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "External API failed.");
                }

                var json = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(json))
                    return Ok(new List<BookSearchResultDto>());

                using var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("docs", out var docs) ||
                    docs.ValueKind != JsonValueKind.Array)
                {
                    return Ok(new List<BookSearchResultDto>());
                }

                var results = new List<BookSearchResultDto>();

                foreach (var item in docs.EnumerateArray().Take(20))
                {
                    try
                    {
                        var key = item.TryGetProperty("key", out var keyProp)
                            ? keyProp.GetString()
                            : null;

                        if (string.IsNullOrWhiteSpace(key))
                            continue;

                        var externalId = key.Replace("/works/", "").Trim();

                        var bookTitle = item.TryGetProperty("title", out var titleProp)
                            ? titleProp.GetString()
                            : null;

                        if (string.IsNullOrWhiteSpace(bookTitle))
                            continue;

                        string author = "Unknown author";

                        if (item.TryGetProperty("author_name", out var authorProp) &&
                            authorProp.ValueKind == JsonValueKind.Array &&
                            authorProp.GetArrayLength() > 0)
                        {
                            author = authorProp[0].GetString() ?? "Unknown author";
                        }

                        string? coverUrl = null;

                        if (item.TryGetProperty("cover_i", out var coverProp) &&
                            coverProp.ValueKind == JsonValueKind.Number)
                        {
                            var coverId = coverProp.GetInt32();
                            coverUrl = $"https://covers.openlibrary.org/b/id/{coverId}-L.jpg";
                        }

                        results.Add(new BookSearchResultDto
                        {
                            ExternalBookId = externalId,
                            Title = bookTitle,
                            Author = author,
                            CoverUrl = coverUrl
                        });
                    }
                    catch
                    {
                        // تجاهل أي عنصر فيه مشكلة
                        continue;
                    }
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Search failed",
                    error = ex.Message
                });
            }
        }
    }
}