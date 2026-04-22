using Microsoft.AspNetCore.Mvc;
using PersonelSistemi.Models;
using System.Collections.Generic;
using System.Linq;

namespace PersonelSistemi.Controllers
{
    public class BPersonelController : Controller
    {
        private readonly AppDbContext _context;

        public BPersonelController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string? adi, string? soyadi, string? siralamaAlani, string? siralamaCiheti)
        {
            var sorgu = _context.BPersoneller.AsQueryable();

            if (!string.IsNullOrEmpty(adi))
                sorgu = sorgu.Where(p => p.adi != null && p.adi.ToLower().Contains(adi.ToLower().Trim()));

            if (!string.IsNullOrEmpty(soyadi))
                sorgu = sorgu.Where(p => p.soyadi != null && p.soyadi.ToLower().Contains(soyadi.ToLower().Trim()));

            if (string.IsNullOrEmpty(siralamaCiheti)) siralamaCiheti = "asc";

            sorgu = siralamaAlani switch
            {
                "adi" => siralamaCiheti == "asc" ? sorgu.OrderBy(p => p.adi) : sorgu.OrderByDescending(p => p.adi),
                "soyadi" => siralamaCiheti == "asc" ? sorgu.OrderBy(p => p.soyadi) : sorgu.OrderByDescending(p => p.soyadi),
                _ => sorgu.OrderBy(p => p.objectid)
            };

            var bPersoneller = sorgu.ToList();

            var kolonlar = new List<GridKolonModel>
            {
                new GridKolonModel { Baslik = "Adı",    VeriAlani = "adi",    FiltrelenebilirMi = true, SiralanabilirMi = true },
                new GridKolonModel { Baslik = "Soyadı", VeriAlani = "soyadi", FiltrelenebilirMi = true, SiralanabilirMi = true }
            };

            var butonlar = new List<GridButonModel>
            {
                new GridButonModel { Metin = "Güncelle", Aksiyon = "Guncelle", CssSinifi = "button warning small", OnayGerektirirMi = false },
                new GridButonModel { Metin = "Sil",      Aksiyon = "Sil",      CssSinifi = "button alert small",   OnayGerektirirMi = true  }
            };

            var dinamikListe = bPersoneller.Select(p =>
            {
                var satir = new Dictionary<string, object?>();
                satir["objectid"] = p.objectid;
                foreach (var kolon in kolonlar)
                    satir[kolon.VeriAlani!] = p.GetType().GetProperty(kolon.VeriAlani!)?.GetValue(p, null);
                return satir;
            }).ToList();

            var model = new GenelGridModel
            {
                Veriler = dinamikListe,
                Kolonlar = kolonlar,
                Butonlar = butonlar,
                ControllerAdi = "BPersonel",
                IsFiltered = true,
                IsPaging = true,
                IsSorted = true,
                SiralamaAlani = siralamaAlani ?? "",
                SiralamaCiheti = siralamaCiheti,
                SayfaBasinaKayitSayisi = 5
            };

            return View("~/Views/Shared/_GenelGrid.cshtml", model);
        }

        [HttpGet]
        public IActionResult Ekle() => View();

        [HttpPost]
        public IActionResult Ekle(BPersonel personel)
        {
            if (!ModelState.IsValid) return View(personel);
            _context.BPersoneller.Add(personel);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Sil(int id)
        {
            var p = _context.BPersoneller.Find(id);
            if (p != null) { _context.BPersoneller.Remove(p); _context.SaveChanges(); }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Guncelle(int id)
        {
            var p = _context.BPersoneller.Find(id);
            if (p == null) return NotFound();
            return View(p);
        }

        [HttpPost]
        public IActionResult Guncelle(BPersonel personel)
        {
            if (!ModelState.IsValid) return View(personel);
            _context.BPersoneller.Update(personel);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
