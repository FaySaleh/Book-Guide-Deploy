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
                client.Timeout = TimeSpan.FromSeconds(10);

                var url = $"https://openlibrary.org/search.json?title={Uri.EscapeDataString(title)}";

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"OpenLibrary failed: {(int)response.StatusCode} - {errorBody}");
                    return StatusCode(503, $"External API failed: {(int)response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("OpenLibrary timeout: " + ex.Message);
                return StatusCode(503, "External API timeout.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("OpenLibrary error: " + ex);
                return StatusCode(500, "Server error while calling external API.");
            }
        }
    }
}