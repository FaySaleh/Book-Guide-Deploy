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
                using var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"OpenLibrary request failed: {errorBody}");
                }

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                if (!doc.RootElement.TryGetProperty("docs", out var docs) || docs.ValueKind != JsonValueKind.Array)
                    return Ok(Array.Empty<BookSearchResultDto>());

                var results = new List<BookSearchResultDto>();

                foreach (var item in docs.EnumerateArray().Take(20))
                {
                    var key = item.TryGetProperty("key", out var keyProp) ? keyProp.GetString() : null;
                    if (string.IsNullOrWhiteSpace(key))
                        continue;

                    var externalId = key.Replace("/works/", "").Trim();

                    var bookTitle = item.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : null;
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

                return Ok(results);
            }
            catch (Exception ex)
            {
                Console.WriteLine("BOOK SEARCH ERROR:");
                Console.WriteLine(ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }
    }
}