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

        
        
        
        [HttpGet]
        public IActionResult KonteynerleriGetir()
        {
            try
            {
                var konteynerler = _context.Konteynerler
                    .Include(k => k.KonteynerAPersoneller)
                        .ThenInclude(kp => kp.Personel)
                    .Include(k => k.KonteynerBPersoneller)
                        .ThenInclude(kp => kp.BPersonel)
                    .ToList();

                var dto = konteynerler.Select(k => new
                {
                    id = k.Id,
                    ad = k.Ad,
                    enlem = k.Enlem,
                    boylam = k.Boylam,
                    aPersoneller = k.KonteynerAPersoneller.Select(kp => new
                    {
                        id = kp.Personel.objectid,
                        adSoyad = kp.Personel.adi + " " + kp.Personel.soyadi,
                        tip = "A"
                    }),
                    bPersoneller = k.KonteynerBPersoneller.Select(kp => new
                    {
                        id = kp.BPersonel.objectid,
                        adSoyad = kp.BPersonel.adi + " " + kp.BPersonel.soyadi,
                        tip = "B"
                    })
                });

                return Json(new { success = true, data = dto });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult KonteynerEkle(Konteyner yeniKonteyner, List<int> secilenAPersoneller, List<int> secilenBPersoneller)
        {
            try
            {
                yeniKonteyner.KayitTarihi = DateTime.UtcNow;

                if (secilenAPersoneller != null && secilenAPersoneller.Any())
                {
                    foreach (var personelId in secilenAPersoneller)
                    {
                        yeniKonteyner.KonteynerAPersoneller.Add(new KonteynerAPersonel { PersonelId = personelId });
                    }
                }

                if (secilenBPersoneller != null && secilenBPersoneller.Any())
                {
                    foreach (var personelId in secilenBPersoneller)
                    {
                        yeniKonteyner.KonteynerBPersoneller.Add(new KonteynerBPersonel { BPersonelId = personelId });
                    }
                }
                yeniKonteyner.Enlem = double.Parse(Request.Form["Enlem"].ToString().Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
                yeniKonteyner.Boylam = double.Parse(Request.Form["Boylam"].ToString().Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);
                _context.Konteynerler.Add(yeniKonteyner);
                _context.SaveChanges();

                var eklenenKonteyner = GetKonteynerDto(yeniKonteyner.Id);
                return Json(new { success = true, data = eklenenKonteyner });
            }
            catch (Exception ex)
            {
                string gercekHata = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = gercekHata });
            }
        }
        private object? GetKonteynerDto(int id)
        {
            var k = _context.Konteynerler
                .Include(x => x.KonteynerAPersoneller)
                    .ThenInclude(kp => kp.Personel)
                .Include(x => x.KonteynerBPersoneller)
                    .ThenInclude(kp => kp.BPersonel)
                .FirstOrDefault(x => x.Id == id);

            if (k == null) return null;

            return new
            {
                id = k.Id,
                ad = k.Ad,
                enlem = k.Enlem,
                boylam = k.Boylam,
                kayitTarihi = k.KayitTarihi,
                aPersoneller = k.KonteynerAPersoneller.Select(kp => new
                {
                    id = kp.Personel.objectid,
                    adSoyad = kp.Personel.adi + " " + kp.Personel.soyadi,
                    tip = "A"
                }).ToList(),
                bPersoneller = k.KonteynerBPersoneller.Select(kp => new
                {
                    id = kp.BPersonel.objectid,
                    adSoyad = kp.BPersonel.adi + " " + kp.BPersonel.soyadi,
                    tip = "B"
                }).ToList()
            };
        }
        [HttpPost]
        public IActionResult KonteynerSil(int id)
        {
            try
            {
                var konteyner = _context.Konteynerler
                    .Include(k => k.KonteynerAPersoneller)
                    .Include(k => k.KonteynerBPersoneller)
                    .FirstOrDefault(k => k.Id == id);

                if (konteyner == null)
                    return Json(new { success = false, message = "Konteyner bulunamadı" });

                _context.KonteynerAPersoneller.RemoveRange(konteyner.KonteynerAPersoneller);
                _context.KonteynerBPersoneller.RemoveRange(konteyner.KonteynerBPersoneller);
                _context.Konteynerler.Remove(konteyner);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult KonteynerPersonelEkle(int konteynerId, int personelId, string personelTipi)
        {
            try
            {
                var konteyner = _context.Konteynerler
                    .Include(x => x.KonteynerAPersoneller)
                    .Include(x => x.KonteynerBPersoneller)
                    .FirstOrDefault(x => x.Id == konteynerId);

                if (konteyner == null)
                    return Json(new { success = false, message = "Konteyner bulunamadı" });

                if (personelTipi == "A")
                {
                    if (konteyner.KonteynerAPersoneller.Any(kp => kp.PersonelId == personelId))
                        return Json(new { success = false, message = "Bu personel zaten ekli" });
                    konteyner.KonteynerAPersoneller.Add(new KonteynerAPersonel { PersonelId = personelId });
                }
                else if (personelTipi == "B")
                {
                    if (konteyner.KonteynerBPersoneller.Any(kp => kp.BPersonelId == personelId))
                        return Json(new { success = false, message = "Bu personel zaten ekli" });
                    konteyner.KonteynerBPersoneller.Add(new KonteynerBPersonel { BPersonelId = personelId });
                }

                _context.SaveChanges();
                var guncelKonteyner = GetKonteynerDto(konteynerId);
                return Json(new { success = true, data = guncelKonteyner });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult KonteynerPersonelCikar(int konteynerId, int personelId, string personelTipi)
        {
            try
            {
                if (personelTipi == "A")
                {
                    var kayit = _context.KonteynerAPersoneller
                        .FirstOrDefault(kp => kp.KonteynerId == konteynerId && kp.PersonelId == personelId);
                    if (kayit != null) _context.KonteynerAPersoneller.Remove(kayit);
                }
                else if (personelTipi == "B")
                {
                    var kayit = _context.KonteynerBPersoneller
                        .FirstOrDefault(kp => kp.KonteynerId == konteynerId && kp.BPersonelId == personelId);
                    if (kayit != null) _context.KonteynerBPersoneller.Remove(kayit);
                }

                _context.SaveChanges();
                var guncelKonteyner = GetKonteynerDto(konteynerId);
                return Json(new { success = true, data = guncelKonteyner });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        

        
        [HttpPost]
        public IActionResult PersonelListesiGuncelle(int insaatId, List<int> aPersonelIds, List<int> bPersonelIds)
        {
            try
            {
                var insaat = _context.Insaatlar
                    .Include(x => x.InsaatPersonelleri)
                    .Include(x => x.InsaatBPersonelleri)
                    .FirstOrDefault(x => x.Id == insaatId);

                if (insaat == null)
                    return Json(new { success = false, message = "İnşaat bulunamadı" });

                // A Personellerini güncelle
                _context.InsaatPersonelleri.RemoveRange(insaat.InsaatPersonelleri);
                if (aPersonelIds != null)
                {
                    foreach (var pid in aPersonelIds)
                    {
                        insaat.InsaatPersonelleri.Add(new InsaatPersonel { PersonelId = pid });
                    }
                }

                // B Personellerini güncelle
                _context.InsaatBPersonelleri.RemoveRange(insaat.InsaatBPersonelleri);
                if (bPersonelIds != null)
                {
                    foreach (var pid in bPersonelIds)
                    {
                        insaat.InsaatBPersonelleri.Add(new InsaatBPersonel { BPersonelId = pid });
                    }
                }

                _context.SaveChanges();

                var guncelInsaat = GetInsaatDto(insaatId);
                return Json(new { success = true, data = guncelInsaat });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        private object? GetInsaatDto(int insaatId)
        {
            var insaat = _context.Insaatlar
                .Include(x => x.InsaatDurumu)
                .Include(x => x.InsaatPersonelleri).ThenInclude(ip => ip.Personel)
                .Include(x => x.InsaatBPersonelleri).ThenInclude(ibp => ibp.BPersonel)
                .FirstOrDefault(x => x.Id == insaatId);

            if (insaat == null) return null;

            var aPersoneller = insaat.InsaatPersonelleri?
                .Where(ip => ip.Personel != null)
                .Select(ip => new
                {
                    id = ip.Personel!.objectid,
                    adSoyad = ip.Personel!.adi + " " + ip.Personel.soyadi,
                    cinsiyet = ip.Personel!.cinsiyet,
                    tip = "A"
                }).ToList();

            var bPersoneller = insaat.InsaatBPersonelleri?
                .Where(ibp => ibp.BPersonel != null)
                .Select(ibp => new
                {
                    id = ibp.BPersonel!.objectid,
                    adSoyad = ibp.BPersonel!.adi + " " + ibp.BPersonel.soyadi,
                    cinsiyet = ibp.BPersonel!.cinsiyet,
                    tip = "B"
                }).ToList();

            return new
            {
                Id = insaat.Id,
                InsaatAdi = insaat.InsaatAdi,
                InsaatTuru = insaat.InsaatTuru,
                KoordinatX = insaat.KoordinatX,
                KoordinatY = insaat.KoordinatY,
                DurumId = insaat.DurumId,
                InsaatDurumu = insaat.InsaatDurumu,
                Aciklama = insaat.Aciklama,
                KayitTarihi = insaat.KayitTarihi,
                BaslamaTarihi = insaat.BaslamaTarihi,
                TamamlanmaYuzdesi = insaat.TamamlanmaYuzdesi,
                APersoneller = aPersoneller,
                BPersoneller = bPersoneller,
                Personeller = aPersoneller
            };
        }
        

    }
}