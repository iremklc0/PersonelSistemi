using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelSistemi.Controllers;
using PersonelSistemi.Models;
using Xunit;

namespace PersonelSistemi.Tests
{
    public class BPersonelControllerTests
    {
        private AppDbContext SahteVeritabaniOlustur()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        
        [Fact]
        public void Ekle_GecerliBPersonel_BasariliEklenmeli()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new BPersonelController(context);
            var yeniPersonel = new BPersonel
            {
                adi = "Mehmet",
                soyadi = "Demir",
                cinsiyet = "Erkek",
                tc = "98765432109",
                sicil_no = "B001"
            };

            var sonuc = controller.Ekle(yeniPersonel);

            Assert.IsType<RedirectToActionResult>(sonuc);
            Assert.Single(context.BPersoneller);
            Assert.Equal("Mehmet", context.BPersoneller.First().adi);
        }

        
        [Fact]
        public void Ekle_AdiBosBPersonel_EklenmemeliVeViewDonmeli()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new BPersonelController(context);
            var bosPersonel = new BPersonel
            {
                adi = null,
                soyadi = "Demir",
                cinsiyet = "Erkek",
                tc = "98765432109",
                sicil_no = "B002"
            };
            controller.ModelState.AddModelError("adi", "Ad zorunludur");

            var sonuc = controller.Ekle(bosPersonel);

            Assert.IsType<ViewResult>(sonuc);
            Assert.Empty(context.BPersoneller);
        }

        
        [Fact]
        public void Sil_VarOlanBPersonel_BasariliSilinmeli()
        {
            var context = SahteVeritabaniOlustur();
            var personel = new BPersonel
            {
                adi = "Test",
                soyadi = "B Kullanıcı",
                cinsiyet = "Kadın",
                tc = "11122233344",
                sicil_no = "B003"
            };
            context.BPersoneller.Add(personel);
            context.SaveChanges();

            var controller = new BPersonelController(context);

            var sonuc = controller.Sil(personel.objectid);

            Assert.IsType<RedirectToActionResult>(sonuc);
            Assert.Empty(context.BPersoneller);
        }

        
        [Fact]
        public void Sil_OlmayanId_HataVermeli()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new BPersonelController(context);

            var sonuc = controller.Sil(99999);

            Assert.IsType<RedirectToActionResult>(sonuc);
        }
    }
}