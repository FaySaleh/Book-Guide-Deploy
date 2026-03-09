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
            var dto = await _dashboard.GetAsync(userId);
            return Ok(dto);
        }
    }
}
