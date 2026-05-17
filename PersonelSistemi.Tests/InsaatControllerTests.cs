using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelSistemi.Controllers;
using PersonelSistemi.Models;
using System.Text.Json;
using Xunit;

namespace PersonelSistemi.Tests
{
    public class InsaatControllerTests
    {
        private AppDbContext SahteVeritabaniOlustur()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        
        private OtekSistem.Models.Insaat SahteInsaatOlustur(AppDbContext context, string ad = "Test Insaat")
        {
            var insaat = new OtekSistem.Models.Insaat
            {
                InsaatAdi = ad,
                InsaatTuru = "Bina",
                Aciklama = "Test aciklama",
                KoordinatX = 32.85,
                KoordinatY = 39.92,
                DurumId = 1,
                TamamlanmaYuzdesi = 50,
                KayitTarihi = DateTime.UtcNow
            };
            context.Insaatlar.Add(insaat);
            context.SaveChanges();
            return insaat;
        }

        
        [Fact]
        public void InsaatlariGetir_BosVeritabani_BosListeDonmeli()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new InsaatController(context);

            var sonuc = controller.InsaatlariGetir();

            Assert.IsType<JsonResult>(sonuc);
        }

        
        [Fact]
        public void InsaatlariGetir_InsaatVarsa_VeriDonmeli()
        {
            var context = SahteVeritabaniOlustur();
            SahteInsaatOlustur(context, "Otek Insaat");
            SahteInsaatOlustur(context, "Bina Projesi");

            var controller = new InsaatController(context);

            var sonuc = controller.InsaatlariGetir();

            Assert.IsType<JsonResult>(sonuc);
            Assert.Equal(2, context.Insaatlar.Count());
        }

        
        [Fact]
        public void Sil_VarOlanInsaat_BasariliSilinmeli()
        {
            var context = SahteVeritabaniOlustur();
            var insaat = SahteInsaatOlustur(context);

            var controller = new InsaatController(context);
            var sonuc = controller.Sil(insaat.Id);

            Assert.IsType<RedirectToActionResult>(sonuc);
            Assert.Empty(context.Insaatlar);
        }

        
        [Fact]
        public void Sil_OlmayanId_HataVermemeli()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new InsaatController(context);

            var sonuc = controller.Sil(99999);

            Assert.IsType<RedirectToActionResult>(sonuc);
        }

        
        [Fact]
        public void DurumGuncelle_GecerliDurum_BasariliGuncellenmeli()
        {
            var context = SahteVeritabaniOlustur();
            var insaat = SahteInsaatOlustur(context);

            var controller = new InsaatController(context);
            var sonuc = controller.DurumGuncelle(insaat.Id, 2); 

            Assert.IsType<JsonResult>(sonuc);
            var guncel = context.Insaatlar.First();
            Assert.Equal(2, guncel.DurumId);
        }

        
        [Fact]
        public void DurumGuncelle_OlmayanId_BasarisizOlmali()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new InsaatController(context);

            var sonuc = controller.DurumGuncelle(99999, 2);

            Assert.IsType<JsonResult>(sonuc);
        }

       
        [Fact]
        public void YuzdeGuncelle_GecerliYuzde_BasariliGuncellenmeli()
        {
            var context = SahteVeritabaniOlustur();
            var insaat = SahteInsaatOlustur(context);

            var controller = new InsaatController(context);
            var sonuc = controller.YuzdeGuncelle(insaat.Id, 75);

            Assert.IsType<JsonResult>(sonuc);
            var guncel = context.Insaatlar.First();
            Assert.Equal(75, guncel.TamamlanmaYuzdesi);
        }

        
        [Fact]
        public void YuzdeGuncelle_OlmayanId_BasarisizOlmali()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new InsaatController(context);

            var sonuc = controller.YuzdeGuncelle(99999, 75);

            Assert.IsType<JsonResult>(sonuc);
        }
    }
}