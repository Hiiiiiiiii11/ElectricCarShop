using Microsoft.AspNetCore.Mvc;

namespace UserAPI.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
