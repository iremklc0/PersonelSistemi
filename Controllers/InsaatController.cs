using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtekSistem.Models;
using PersonelSistemi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PersonelSistemi.Controllers
{
    public class InsaatController : Controller
    {
        private readonly AppDbContext _context;

        public InsaatController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var kolonlar = new List<GridKolonModel>
            {
                new GridKolonModel { Baslik = "İnşaat Adı",     VeriAlani = "insaatAdi",          FiltrelenebilirMi = true,  SiralanabilirMi = true },
                new GridKolonModel { Baslik = "Türü",           VeriAlani = "insaatTuru",          FiltrelenebilirMi = true,  SiralanabilirMi = true },
                new GridKolonModel { Baslik = "Durum",          VeriAlani = "durumAdi",            FiltrelenebilirMi = true,  SiralanabilirMi = true },
                new GridKolonModel { Baslik = "Başlama Tarihi", VeriAlani = "baslamaTarihiStr",    FiltrelenebilirMi = false, SiralanabilirMi = true },
                new GridKolonModel { Baslik = "Tamamlanma %",   VeriAlani = "tamamlanmaYuzdesi",   FiltrelenebilirMi = false, SiralanabilirMi = true },
            };

            var insaatlar = _context.Insaatlar
                .Include(x => x.InsaatDurumu)
                .Include(x => x.InsaatPersonelleri).ThenInclude(ip => ip.Personel)
                .Include(x => x.InsaatBPersonelleri).ThenInclude(ibp => ibp.BPersonel)
                .ToList()
                .Select(x => new Dictionary<string, object>
                {
                    ["id"] = x.Id,
                    ["insaatAdi"] = x.InsaatAdi ?? "",
                    ["insaatTuru"] = x.InsaatTuru ?? "",
                    ["durumAdi"] = x.DurumId == 0 ? "Durduruldu" : x.DurumId == 1 ? "Devam Ediyor" : "Tamamlandı",
                    ["durumId"] = x.DurumId,
                    ["baslamaTarihiStr"] = x.BaslamaTarihi.HasValue ? x.BaslamaTarihi.Value.ToLocalTime().ToString("dd.MM.yyyy") : "-",
                    ["tamamlanmaYuzdesi"] = "%" + x.TamamlanmaYuzdesi,
                    ["aciklama"] = x.Aciklama ?? "",
                    ["aPersoneller"] = x.InsaatPersonelleri.Where(ip => ip.Personel != null)
                                               .Select(ip => ip.Personel!.adi + " " + ip.Personel!.soyadi).ToList(),
                    ["bPersoneller"] = x.InsaatBPersonelleri.Where(ibp => ibp.BPersonel != null)
                                               .Select(ibp => ibp.BPersonel!.adi + " " + ibp.BPersonel!.soyadi).ToList(),
                }).ToList();

            ViewBag.Kolonlar = kolonlar;
            ViewBag.Veriler = insaatlar;
            ViewBag.Baslik = "İnşaat Listesi";

            return View();
        }



        [HttpPost]
        public IActionResult InsaatEkle(Insaat yeniInsaat, List<int> secilenAPersoneller, List<int> secilenBPersoneller)
        {
            try
            {
                yeniInsaat.KayitTarihi = DateTime.UtcNow;

                if (secilenAPersoneller != null && secilenAPersoneller.Any())
                {
                    foreach (var personelId in secilenAPersoneller)
                    {
                        yeniInsaat.InsaatPersonelleri.Add(new InsaatPersonel { PersonelId = personelId });
                    }
                }

                if (secilenBPersoneller != null && secilenBPersoneller.Any())
                {
                    foreach (var bPersonelId in secilenBPersoneller)
                    {
                        yeniInsaat.InsaatBPersonelleri.Add(new InsaatBPersonel { BPersonelId = bPersonelId });
                    }
                }
                if (yeniInsaat.BaslamaTarihi.HasValue)
                    yeniInsaat.BaslamaTarihi = yeniInsaat.BaslamaTarihi.Value.ToUniversalTime();
                _context.Insaatlar.Add(yeniInsaat);
                _context.SaveChanges();

                var eklenenInsaat = GetInsaatDto(yeniInsaat.Id);


                if (eklenenInsaat == null)
                    return Json(new { success = false, message = "İnşaat kaydedildi ama veri çekilemedi" });

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
                    .Include(x => x.InsaatPersonelleri).ThenInclude(ip => ip.Personel)
                    .Include(x => x.InsaatBPersonelleri).ThenInclude(ibp => ibp.BPersonel)
                    .Select(x => new
                    {
                        x.Id,
                        x.InsaatAdi,
                        x.InsaatTuru,
                        x.KoordinatX,
                        x.KoordinatY,
                        x.DurumId,
                        x.InsaatDurumu,
                        x.Aciklama,
                        x.KayitTarihi,
                        x.BaslamaTarihi,
                        x.TamamlanmaYuzdesi,

                        APersoneller = x.InsaatPersonelleri.Select(ip => new
                        {
                            id = ip.Personel!.objectid,
                            adSoyad = ip.Personel!.adi + " " + ip.Personel!.soyadi,
                            cinsiyet = ip.Personel!.cinsiyet,
                            tip = "A"
                        }).ToList(),
                        BPersoneller = x.InsaatBPersonelleri.Select(ibp => new
                        {
                            id = ibp.BPersonel!.objectid,
                            adSoyad = ibp.BPersonel!.adi + " " + ibp.BPersonel!.soyadi,
                            cinsiyet = ibp.BPersonel!.cinsiyet,
                            tip = "B"
                        }).ToList(),

                        Personeller = x.InsaatPersonelleri.Select(ip => new
                        {
                            id = ip.Personel!.objectid,
                            adSoyad = ip.Personel!.adi + " " + ip.Personel!.soyadi,
                            cinsiyet = ip.Personel!.cinsiyet,
                            tip = "A"
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
            var insaat = _context.Insaatlar
                .Include(x => x.InsaatPersonelleri)
                .Include(x => x.InsaatBPersonelleri)
                .FirstOrDefault(x => x.Id == id);

            if (insaat == null) return Json(new { success = false });

            _context.InsaatPersonelleri.RemoveRange(insaat.InsaatPersonelleri);
            _context.InsaatBPersonelleri.RemoveRange(insaat.InsaatBPersonelleri);
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
        [HttpPost]
        public IActionResult YuzdeGuncelle(int id, int yuzde)
        {
            var insaat = _context.Insaatlar.Find(id);
            if (insaat == null) return Json(new { success = false });
            insaat.TamamlanmaYuzdesi = yuzde;
            _context.SaveChanges();
            var guncelInsaat = GetInsaatDto(id);
            return Json(new { success = true, data = guncelInsaat });
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
        cinsiyet = p.cinsiyet,
        tip = "A"
    }).ToList();

                var bPersoneller = _context.BPersoneller
                    .Where(p => p.adi != null && p.soyadi != null)
                    .Select(p => new {
                        id = p.objectid,
                        adSoyad = p.adi + " " + p.soyadi,
                        cinsiyet = p.cinsiyet,
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




        [HttpPost]
        public IActionResult PersonelEkle(int insaatId, int personelId, string personelTipi)
        {
            try
            {
                var insaat = _context.Insaatlar
                    .Include(x => x.InsaatPersonelleri)
                    .Include(x => x.InsaatBPersonelleri)
                    .FirstOrDefault(x => x.Id == insaatId);

                if (insaat == null)
                    return Json(new { success = false, message = "İnşaat bulunamadı" });

                if (personelTipi == "A")
                {

                    if (insaat.InsaatPersonelleri.Any(ip => ip.PersonelId == personelId))
                        return Json(new { success = false, message = "Bu personel zaten atanmış" });

                    insaat.InsaatPersonelleri.Add(new InsaatPersonel { PersonelId = personelId });
                }
                else if (personelTipi == "B")
                {
                    if (insaat.InsaatBPersonelleri.Any(ibp => ibp.BPersonelId == personelId))
                        return Json(new { success = false, message = "Bu personel zaten atanmış" });

                    insaat.InsaatBPersonelleri.Add(new InsaatBPersonel { BPersonelId = personelId });
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


        /// İnşaattan personel çıkar

        [HttpPost]
        public IActionResult PersonelCikar(int insaatId, int personelId, string personelTipi)
        {
            try
            {
                if (personelTipi == "A")
                {
                    var kayit = _context.InsaatPersonelleri
                        .FirstOrDefault(ip => ip.InsaatId == insaatId && ip.PersonelId == personelId);

                    if (kayit != null)
                        _context.InsaatPersonelleri.Remove(kayit);
                }
                else if (personelTipi == "B")
                {
                    var kayit = _context.InsaatBPersonelleri
                        .FirstOrDefault(ibp => ibp.InsaatId == insaatId && ibp.BPersonelId == personelId);

                    if (kayit != null)
                        _context.InsaatBPersonelleri.Remove(kayit);
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