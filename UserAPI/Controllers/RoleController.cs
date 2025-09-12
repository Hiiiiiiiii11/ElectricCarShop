using Microsoft.AspNetCore.Mvc;

namespace UserAPI.Controllers
{
    public class RoleController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
