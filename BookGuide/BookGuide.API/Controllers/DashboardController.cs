using BookGuide.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookGuide.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboard;

        public DashboardController(DashboardService dashboard)
        {
            _dashboard = dashboard;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(int userId)
        {
            try
            {
                var dto = await _dashboard.GetAsync(userId);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine("DASHBOARD ERROR:");
                Console.WriteLine(ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }
    }
}