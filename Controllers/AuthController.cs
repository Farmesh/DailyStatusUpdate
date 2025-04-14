using Microsoft.AspNetCore.Mvc;

namespace DailyStatusApp.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
