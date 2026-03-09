using Microsoft.AspNetCore.Mvc;

namespace BookGuide.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
