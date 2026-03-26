using BookGuide.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookGuide.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetDashboard(int userId)
        {
            try
            {
                var result = await _dashboardService.GetDashboardAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while loading dashboard data.",
                    error = ex.Message
                });
            }
        }
    }
}