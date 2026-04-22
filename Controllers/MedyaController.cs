using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PersonelSistemi.Controllers
{
    public class MedyaController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> DosyaYukle(List<IFormFile> yuklenenDosyalar)
        {
            if (yuklenenDosyalar == null || yuklenenDosyalar.Count == 0)
            {
                return BadRequest("Lütfen yüklenecek bir dosya seçin!");
            }

            var yuklemeYolu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(yuklemeYolu))
            {
                Directory.CreateDirectory(yuklemeYolu);
            }

            foreach (var dosya in yuklenenDosyalar)
            {
                if (dosya.Length > 0)
                {
                    var dosyaYolu = Path.Combine(yuklemeYolu, dosya.FileName);
                    using (var stream = new FileStream(dosyaYolu, FileMode.Create))
                    {
                        await dosya.CopyToAsync(stream);
                    }
                }
            }

            return Ok(new { mesaj = $"{yuklenenDosyalar.Count} adet dosya başarıyla yüklendi!" });
        }

        [HttpPost]
        public IActionResult DosyaSil(string dosyaAdi)
        {
            try
            {
                if (!string.IsNullOrEmpty(dosyaAdi))
                {
                    var yol = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", dosyaAdi);
                    if (System.IO.File.Exists(yol))
                    {
                        System.IO.File.Delete(yol);
                        return Ok(new { mesaj = "Silindi" });
                    }
                }
                return BadRequest("Dosya bulunamadı.");
            }
            catch (Exception ex)
            {
                return BadRequest("Hata: " + ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GaleriYenile()
        {
            return PartialView("_GaleriPartial");
        }
    }
}
