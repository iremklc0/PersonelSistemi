using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtekSistem.Models;
using PersonelSistemi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MiniSoftware;

namespace PersonelSistemi.Controllers
{
    public class InsaatController : Controller
    {
        private readonly AppDbContext _context;

        public InsaatController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string? insaatAdi, string? insaatTuru, string? siralamaAlani, string? siralamaCiheti)
        {
            var sorgu = _context.Insaatlar
                .Include(x => x.InsaatDurumu)
                .AsQueryable();

            if (!string.IsNullOrEmpty(insaatAdi))
                sorgu = sorgu.Where(x => x.InsaatAdi != null && x.InsaatAdi.ToLower().Contains(insaatAdi.ToLower().Trim()));

            if (!string.IsNullOrEmpty(insaatTuru))
                sorgu = sorgu.Where(x => x.InsaatTuru != null && x.InsaatTuru.ToLower().Contains(insaatTuru.ToLower().Trim()));

            if (string.IsNullOrEmpty(siralamaCiheti)) siralamaCiheti = "asc";

            sorgu = siralamaAlani switch
            {
                "insaatAdi" => siralamaCiheti == "asc" ? sorgu.OrderBy(x => x.InsaatAdi) : sorgu.OrderByDescending(x => x.InsaatAdi),
                "insaatTuru" => siralamaCiheti == "asc" ? sorgu.OrderBy(x => x.InsaatTuru) : sorgu.OrderByDescending(x => x.InsaatTuru),
                "baslamaTarihi" => siralamaCiheti == "asc" ? sorgu.OrderBy(x => x.BaslamaTarihi) : sorgu.OrderByDescending(x => x.BaslamaTarihi),
                "tamamlanmaYuzdesi" => siralamaCiheti == "asc" ? sorgu.OrderBy(x => x.TamamlanmaYuzdesi) : sorgu.OrderByDescending(x => x.TamamlanmaYuzdesi),
                _ => sorgu.OrderBy(x => x.Id)
            };

            var insaatlar = sorgu.ToList();

            var kolonlar = new List<GridKolonModel>
    {
        new GridKolonModel { Baslik = "İnşaat Adı",     VeriAlani = "insaatAdi",          FiltrelenebilirMi = true,  SiralanabilirMi = true },
        new GridKolonModel { Baslik = "Türü",           VeriAlani = "insaatTuru",         FiltrelenebilirMi = true,  SiralanabilirMi = true },
        new GridKolonModel { Baslik = "Durum",          VeriAlani = "durumAdi",           FiltrelenebilirMi = false, SiralanabilirMi = false },
        new GridKolonModel { Baslik = "Başlama Tarihi", VeriAlani = "baslamaTarihi",      FiltrelenebilirMi = false, SiralanabilirMi = true },
        new GridKolonModel { Baslik = "Tamamlanma %",   VeriAlani = "tamamlanmaYuzdesi",  FiltrelenebilirMi = false, SiralanabilirMi = true }
    };

            var butonlar = new List<GridButonModel>
    {
        new GridButonModel { Metin = "Rapor",    Aksiyon = "Rapor",    CssSinifi = "button primary small", OnayGerektirirMi = false },
        new GridButonModel { Metin = "Sil",      Aksiyon = "Sil",      CssSinifi = "button alert small",   OnayGerektirirMi = true  }
    };

            var dinamikListe = insaatlar.Select(x =>
            {
                var satir = new Dictionary<string, object?>();
                satir["objectid"] = x.Id;
                satir["insaatAdi"] = x.InsaatAdi ?? "";
                satir["insaatTuru"] = x.InsaatTuru ?? "";
                satir["durumAdi"] = x.DurumId == 0 ? "🔴 Durduruldu" : x.DurumId == 1 ? "🟡 Devam Ediyor" : "🟢 Tamamlandı";
                satir["baslamaTarihi"] = x.BaslamaTarihi.HasValue ? x.BaslamaTarihi.Value.ToLocalTime().ToString("dd.MM.yyyy") : "-";
                satir["tamamlanmaYuzdesi"] = "%" + x.TamamlanmaYuzdesi;
                return satir;
            }).ToList();

            var model = new GenelGridModel
            {
                Veriler = dinamikListe,
                Kolonlar = kolonlar,
                Butonlar = butonlar,
                ControllerAdi = "Insaat",
                IsFiltered = true,
                IsPaging = true,
                IsSorted = true,
                SiralamaAlani = siralamaAlani ?? "",
                SiralamaCiheti = siralamaCiheti,
                SayfaBasinaKayitSayisi = 10,
                YeniKayitButonuGoster = false
            };

            return View("~/Views/Shared/_GenelGrid.cshtml", model);
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
        public IActionResult Sil(int id)
        {
            var insaat = _context.Insaatlar
                .Include(x => x.InsaatPersonelleri)
                .Include(x => x.InsaatBPersonelleri)
                .FirstOrDefault(x => x.Id == id);

            if (insaat != null)
            {
                _context.InsaatPersonelleri.RemoveRange(insaat.InsaatPersonelleri);
                _context.InsaatBPersonelleri.RemoveRange(insaat.InsaatBPersonelleri);
                _context.Insaatlar.Remove(insaat);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
        public IActionResult Rapor(int id)
        {
            var insaat = _context.Insaatlar
                .Include(x => x.InsaatDurumu)
                .Include(x => x.InsaatPersonelleri).ThenInclude(ip => ip.Personel)
                .Include(x => x.InsaatBPersonelleri).ThenInclude(ibp => ibp.BPersonel)
                .FirstOrDefault(x => x.Id == id);

            if (insaat == null) return NotFound();

            var aPersoneller = insaat.InsaatPersonelleri
                .Where(ip => ip.Personel != null)
                .Select(ip => ip.Personel!.adi + " " + ip.Personel!.soyadi)
                .ToList();

            var bPersoneller = insaat.InsaatBPersonelleri
                .Where(ibp => ibp.BPersonel != null)
                .Select(ibp => ibp.BPersonel!.adi + " " + ibp.BPersonel!.soyadi)
                .ToList();

            ViewBag.Insaat = insaat;
            ViewBag.APersoneller = aPersoneller.Any() ? string.Join(", ", aPersoneller) : "Yok";
            ViewBag.BPersoneller = bPersoneller.Any() ? string.Join(", ", bPersoneller) : "Yok";

            return View();
        }


        public IActionResult RaporPdf(int id)
        {
            var insaat = _context.Insaatlar
                .Include(x => x.InsaatDurumu)
                .Include(x => x.InsaatPersonelleri).ThenInclude(ip => ip.Personel)
                .Include(x => x.InsaatBPersonelleri).ThenInclude(ibp => ibp.BPersonel)
                .FirstOrDefault(x => x.Id == id);

            if (insaat == null) return NotFound();

            var aPersoneller = insaat.InsaatPersonelleri
                .Where(ip => ip.Personel != null)
                .Select(ip => ip.Personel!.adi + " " + ip.Personel!.soyadi)
                .ToList();

            var bPersoneller = insaat.InsaatBPersonelleri
                .Where(ibp => ibp.BPersonel != null)
                .Select(ibp => ibp.BPersonel!.adi + " " + ibp.BPersonel!.soyadi)
                .ToList();

            var durum = insaat.DurumId == 0 ? "Durduruldu" : insaat.DurumId == 1 ? "Devam Ediyor" : "Tamamlandı";
            var tarih = insaat.BaslamaTarihi.HasValue ? insaat.BaslamaTarihi.Value.ToLocalTime().ToString("dd.MM.yyyy") : "Belirtilmemiş";
            var logoPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "otek-logo.png.webp");

            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(t => t.FontSize(11));

                    // ÜST KISIM - LOGO + BAŞLIK
                    page.Header().Column(col =>
                    {
                        if (System.IO.File.Exists(logoPath))
                        {
                            col.Item().AlignCenter().Height(70).Image(logoPath);
                        }
                        col.Item().PaddingTop(10).Text("İnşaat Durum Raporu")
                            .FontSize(18).Bold().AlignCenter().FontColor(Colors.Grey.Darken3);
                        col.Item().PaddingBottom(10).LineHorizontal(2).LineColor(Colors.Grey.Darken3);
                    });

                    // İÇERİK - TABLO
                    page.Content().PaddingVertical(15).Column(col =>
                    {
                        col.Item().PaddingBottom(10).Text(insaat.InsaatAdi ?? "")
                            .FontSize(16).Bold().FontColor(Colors.Blue.Darken2);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(4);
                            });

                            void Satir(string baslik, string deger, bool gri = false)
                            {
                                var arkaplan = gri ? Colors.Grey.Lighten3 : Colors.White;
                                table.Cell().Background(Colors.Grey.Lighten2).Border(1).BorderColor(Colors.Grey.Medium).Padding(8).Text(baslik).Bold();
                                table.Cell().Background(arkaplan).Border(1).BorderColor(Colors.Grey.Medium).Padding(8).Text(deger);
                            }

                            Satir("Tür:", insaat.InsaatTuru ?? "-");
                            Satir("Durum:", durum, true);
                            Satir("Açıklama:", insaat.Aciklama ?? "-");
                            Satir("Başlama Tarihi:", tarih, true);
                            Satir("Tamamlanma:", "%" + insaat.TamamlanmaYuzdesi);
                            Satir("A Personelleri:", aPersoneller.Any() ? string.Join(", ", aPersoneller) : "Yok", true);
                            Satir("B Personelleri:", bPersoneller.Any() ? string.Join(", ", bPersoneller) : "Yok");
                        });
                    });

                    // ALT KISIM - TARİH
                    page.Footer().AlignRight().Text(t =>
                    {
                        t.Span("Rapor Tarihi: ").FontSize(9).FontColor(Colors.Grey.Darken1);
                        t.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "InsaatRaporu_" + insaat.InsaatAdi + ".pdf");
        }
        public IActionResult RaporWord(int id)
        {
            var insaat = _context.Insaatlar
                .Include(x => x.InsaatDurumu)
                .Include(x => x.InsaatPersonelleri).ThenInclude(ip => ip.Personel)
                .Include(x => x.InsaatBPersonelleri).ThenInclude(ibp => ibp.BPersonel)
                .FirstOrDefault(x => x.Id == id);

            if (insaat == null) return NotFound();

            var aPersoneller = insaat.InsaatPersonelleri
                .Where(ip => ip.Personel != null)
                .Select(ip => ip.Personel!.adi + " " + ip.Personel!.soyadi)
                .ToList();

            var bPersoneller = insaat.InsaatBPersonelleri
                .Where(ibp => ibp.BPersonel != null)
                .Select(ibp => ibp.BPersonel!.adi + " " + ibp.BPersonel!.soyadi)
                .ToList();

            var durum = insaat.DurumId == 0 ? "Durduruldu" : insaat.DurumId == 1 ? "Devam Ediyor" : "Tamamlandı";
            var tarih = insaat.BaslamaTarihi.HasValue ? insaat.BaslamaTarihi.Value.ToLocalTime().ToString("dd.MM.yyyy") : "Belirtilmemiş";

            var templatePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "rapor-sablonu.docx");
            if (!System.IO.File.Exists(templatePath))
            {
                return Content("Şablon dosyası bulunamadı: " + templatePath);
            }

            var veri = new Dictionary<string, object>
            {
                ["insaatAdi"] = insaat.InsaatAdi ?? "-",
                ["insaatTuru"] = insaat.InsaatTuru ?? "-",
                ["durum"] = durum,
                ["aciklama"] = insaat.Aciklama ?? "-",
                ["baslamaTarihi"] = tarih,
                ["tamamlanmaYuzdesi"] = "%" + insaat.TamamlanmaYuzdesi,
                ["aPersoneller"] = aPersoneller.Any() ? string.Join(", ", aPersoneller) : "Yok",
                ["bPersoneller"] = bPersoneller.Any() ? string.Join(", ", bPersoneller) : "Yok",
                ["raporTarihi"] = DateTime.Now.ToString("dd.MM.yyyy HH:mm")
            };

            using (var memoryStream = new MemoryStream())
            {
                MiniWord.SaveAsByTemplate(memoryStream, templatePath, veri);
                var wordBytes = memoryStream.ToArray();
                return File(wordBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                            "InsaatRaporu_" + insaat.InsaatAdi + ".docx");
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