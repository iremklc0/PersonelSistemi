using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelSistemi.Controllers;
using PersonelSistemi.Models;
using Xunit;

namespace PersonelSistemi.Tests
{
    public class PersonelControllerTests
    {
        
        private AppDbContext SahteVeritabaniOlustur()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        
        [Fact]
        public void Ekle_GecerliPersonel_BasariliEklenmeli()
        {
            
            var context = SahteVeritabaniOlustur();
            var controller = new PersonelController(context);
            var yeniPersonel = new Personel
            {
                adi = "Ahmet",
                soyadi = "Yılmaz",
                cinsiyet = "Erkek",
                tc = "12345678901",
                personel_id = "P001"
            };

            
            var sonuc = controller.Ekle(yeniPersonel);

            
            Assert.IsType<RedirectToActionResult>(sonuc);
            Assert.Single(context.Personeller);
            Assert.Equal("Ahmet", context.Personeller.First().adi);
        }

        
        [Fact]
        public void Ekle_AdiBosPersonel_EklenmemeliVeViewDonmeli()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new PersonelController(context);
            var bosPersonel = new Personel
            {
                adi = null,
                soyadi = "Yılmaz",
                cinsiyet = "Erkek",
                tc = "12345678901",
                personel_id = "P002"

            };
            controller.ModelState.AddModelError("adi", "Ad zorunludur");

            var sonuc = controller.Ekle(bosPersonel);

            Assert.IsType<ViewResult>(sonuc); 
            Assert.Empty(context.Personeller); 
        }

        
        [Fact]
        public void Sil_VarOlanPersonel_BasariliSilinmeli()
        {
            var context = SahteVeritabaniOlustur();
            var personel = new Personel
            {
                adi = "Test",
                soyadi = "Kullanıcı",
                cinsiyet = "Kadın",
                 tc = "12345678901",
                personel_id = "P003"
            };
            context.Personeller.Add(personel);
            context.SaveChanges();

            var controller = new PersonelController(context);

            var sonuc = controller.Sil(personel.objectid);

            Assert.IsType<RedirectToActionResult>(sonuc);
            Assert.Empty(context.Personeller); 
        }

        
        [Fact]
        public void Sil_OlmayanId_HataVermeli()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new PersonelController(context);

            var sonuc = controller.Sil(99999);

            Assert.IsType<RedirectToActionResult>(sonuc);
        }
    }
}