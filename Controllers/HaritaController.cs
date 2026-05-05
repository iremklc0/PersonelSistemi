using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtekSistem.Models;
using PersonelSistemi.Models;
using System;
using System.Collections.Generic;
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
        public IActionResult InsaatEkle(Insaat yeniInsaat, List<int> secilenPersoneller)
        {
            try
            {
                yeniInsaat.KayitTarihi = DateTime.UtcNow;

                if (secilenPersoneller != null && secilenPersoneller.Any())
                {
                    foreach (var personelId in secilenPersoneller)
                    {
                        yeniInsaat.InsaatPersonelleri.Add(new InsaatPersonel { PersonelId = personelId });
                    }
                }

                _context.Insaatlar.Add(yeniInsaat);
                _context.SaveChanges();

                var eklenenInsaat = _context.Insaatlar
                    .Include(x => x.InsaatDurumu)
                    .Include(x => x.InsaatPersonelleri).ThenInclude(ip => ip.Personel)
                    .Select(x => new {
                        x.Id,
                        x.InsaatAdi,
                        x.InsaatTuru,
                        x.KoordinatX,
                        x.KoordinatY,
                        x.DurumId,
                        x.InsaatDurumu,
                        x.Aciklama,
                        x.KayitTarihi,
                        Personeller = x.InsaatPersonelleri.Select(ip => new {
                            id = ip.Personel.objectid,
                            adSoyad = ip.Personel.adi + " " + ip.Personel.soyadi
                        }).ToList()
                    })
                    .FirstOrDefault(x => x.Id == yeniInsaat.Id);

                return Json(new { success = true, data = eklenenInsaat });
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
                var insaatlar = _context.Insaatlar
                    .Include(x => x.InsaatDurumu)
                    .Include(x => x.InsaatPersonelleri)
                        .ThenInclude(ip => ip.Personel)
                    .Select(x => new {
                        x.Id,
                        x.InsaatAdi,
                        x.InsaatTuru,
                        x.KoordinatX,
                        x.KoordinatY,
                        x.DurumId,
                        x.InsaatDurumu,
                        x.Aciklama,
                        x.KayitTarihi,
                        Personeller = x.InsaatPersonelleri.Select(ip => new {
                            id = ip.Personel.objectid,
                            adSoyad = ip.Personel.adi + " " + ip.Personel.soyadi
                        }).ToList()
                    })
                    .ToList();
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


        [HttpGet]
        public IActionResult PersonelleriGetir()
        {
            try
            {
                var aPersoneller = _context.Personeller
                    .Where(p => p.adi != null && p.soyadi != null)
                    .Select(p => new {
                        id = p.objectid,
                        adSoyad = p.adi + " " + p.soyadi,
                        tip = "A"
                    }).ToList();

                var bPersoneller = _context.BPersoneller
                    .Where(p => p.adi != null && p.soyadi != null)
                    .Select(p => new {
                        id = p.objectid,
                        adSoyad = p.adi + " " + p.soyadi,
                        tip = "B"
                    }).ToList();

                return Json(new
                {
                    success = true,
                    aPersoneller = aPersoneller,
                    bPersoneller = bPersoneller
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}