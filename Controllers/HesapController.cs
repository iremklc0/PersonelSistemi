using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PersonelSistemi.Models;

namespace PersonelSistemi.Controllers
{
    public class HesapController : Controller
    {
        private readonly AppDbContext _context;

        public HesapController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult SifreGuncelle(string EskiSifre, string YeniSifre, string YeniSifreTekrar)
        {
            if (string.IsNullOrEmpty(EskiSifre) || string.IsNullOrEmpty(YeniSifre) || string.IsNullOrEmpty(YeniSifreTekrar))
            {
                TempData["Hata"] = "Lütfen tüm alanları doldurun!";
                return RedirectToAction("Index", "Home");
            }

            if (YeniSifre != YeniSifreTekrar)
            {
                TempData["Hata"] = "Yeni şifreler birbiriyle uyuşmuyor!";
                return RedirectToAction("Index", "Home");
            }

            var kullanici = _context.Kullanicilar.FirstOrDefault(k => k.KullaniciAdi == "admin");

            if (kullanici == null)
            {
                TempData["Hata"] = "Kullanıcı bulunamadı!";
                return RedirectToAction("Index", "Home");
            }

            if (kullanici.Sifre != EskiSifre)
            {
                TempData["Hata"] = "Eski şifrenizi yanlış girdiniz!";
                return RedirectToAction("Index", "Home");
            }

            kullanici.Sifre = YeniSifre;

            _context.Kullanicilar.Update(kullanici);
            _context.SaveChanges();

            TempData["Basari"] = "Şifreniz başarıyla güncellendi!";
            return RedirectToAction("Index", "Home");
        }
    }
}