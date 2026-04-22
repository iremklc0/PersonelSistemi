using Microsoft.AspNetCore.Mvc;

namespace PersonelSistemi.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }
}