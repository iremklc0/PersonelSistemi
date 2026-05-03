using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtekSistem.Models;
using PersonelSistemi.Models;
using System;
using System.Linq; 

namespace PersonelSistemi.Controllers
{
    public class HaritaController : Controller
    {
        private readonly AppDbContext _context;

        public HaritaController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult InsaatEkle(Insaat yeniInsaat)
        {
            try
            {
                
                yeniInsaat.KayitTarihi = DateTime.UtcNow;

                _context.Insaatlar.Add(yeniInsaat);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                string gercekHata = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = gercekHata });
            }
        }

       
        [HttpGet]
        public IActionResult InsaatlariGetir()
        {
            try
            {
                var insaatlar = _context.Insaatlar.Include(x => x.InsaatDurumu).ToList();
                return Json(new { success = true, data = insaatlar });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult InsaatSil(int id)
        {
            var insaat = _context.Insaatlar.Find(id);
            if (insaat == null) return Json(new { success = false });

            _context.Insaatlar.Remove(insaat);
            _context.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult DurumGuncelle(int id, int yeniDurumId)
        {
            var insaat = _context.Insaatlar.Find(id);
            if (insaat == null) return Json(new { success = false });

            insaat.DurumId = yeniDurumId;
            _context.SaveChanges();
            return Json(new { success = true });
        }
    }
}
