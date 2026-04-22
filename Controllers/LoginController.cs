using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PersonelSistemi.Models;

namespace PersonelSistemi.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!_context.Kullanicilar.Any(k => k.KullaniciAdi == "admin"))
            {
                _context.Kullanicilar.Add(new Kullanici { KullaniciAdi = "admin", Sifre = "123" });
                _context.SaveChanges();
            }

            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var kullanici = _context.Kullanicilar.FirstOrDefault(k => k.KullaniciAdi == model.KullaniciAdi && k.Sifre == model.Sifre);

                if (kullanici != null)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı!");
                }
            }

            return View(model);
        }

        public IActionResult CikisYap()
        {
            return RedirectToAction("Index", "Login");
        }
    }
}