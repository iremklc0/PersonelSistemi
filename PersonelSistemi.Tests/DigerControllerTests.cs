using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PersonelSistemi.Controllers;
using PersonelSistemi.Models;
using Xunit;

namespace PersonelSistemi.Tests
{
    public class DigerControllerTests
    {
        private AppDbContext SahteVeritabaniOlustur()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        
        private ITempDataDictionary SahteTempDataOlustur()
        {
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new Moq.Mock<ITempDataProvider>().Object;
            return new TempDataDictionary(httpContext, tempDataProvider);
        }

        // ===== HOME CONTROLLER =====

        [Fact]
        public void Home_Index_ViewDonmeli()
        {
            var controller = new HomeController();

            var sonuc = controller.Index();

            Assert.IsType<ViewResult>(sonuc);
        }

        // ===== HESAP CONTROLLER =====

        [Fact]
        public void SifreGuncelle_BosAlanlarla_HomeYonlendirmeli()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new HesapController(context);
            controller.TempData = SahteTempDataOlustur();

            var sonuc = controller.SifreGuncelle("", "yeni123", "yeni123");

            var yonlendirme = Assert.IsType<RedirectToActionResult>(sonuc);
            Assert.Equal("Index", yonlendirme.ActionName);
            Assert.Equal("Home", yonlendirme.ControllerName);
            Assert.Equal("Lütfen tüm alanları doldurun!", controller.TempData["Hata"]);
        }

        [Fact]
        public void SifreGuncelle_YeniSifrelerUyusmuyor_HataDonmeli()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new HesapController(context);
            controller.TempData = SahteTempDataOlustur();

            var sonuc = controller.SifreGuncelle("eski123", "yeni123", "farkli456");

            Assert.IsType<RedirectToActionResult>(sonuc);
            Assert.Equal("Yeni şifreler birbiriyle uyuşmuyor!", controller.TempData["Hata"]);
        }

        [Fact]
        public void SifreGuncelle_KullaniciYok_HataDonmeli()
        {
            var context = SahteVeritabaniOlustur();
            
            var controller = new HesapController(context);
            controller.TempData = SahteTempDataOlustur();

            var sonuc = controller.SifreGuncelle("eski123", "yeni123", "yeni123");

            Assert.IsType<RedirectToActionResult>(sonuc);
            Assert.Equal("Kullanıcı bulunamadı!", controller.TempData["Hata"]);
        }

        [Fact]
        public void SifreGuncelle_EskiSifreYanlis_HataDonmeli()
        {
            var context = SahteVeritabaniOlustur();
            context.Kullanicilar.Add(new Kullanici { KullaniciAdi = "admin", Sifre = "dogru123" });
            context.SaveChanges();

            var controller = new HesapController(context);
            controller.TempData = SahteTempDataOlustur();

            var sonuc = controller.SifreGuncelle("yanlis", "yeni123", "yeni123");

            Assert.IsType<RedirectToActionResult>(sonuc);
            Assert.Equal("Eski şifrenizi yanlış girdiniz!", controller.TempData["Hata"]);
        }

        [Fact]
        public void SifreGuncelle_DogruBilgilerle_BasariliGuncellenmeli()
        {
            var context = SahteVeritabaniOlustur();
            context.Kullanicilar.Add(new Kullanici { KullaniciAdi = "admin", Sifre = "eski123" });
            context.SaveChanges();

            var controller = new HesapController(context);
            controller.TempData = SahteTempDataOlustur();

            var sonuc = controller.SifreGuncelle("eski123", "yeni456", "yeni456");

            Assert.IsType<RedirectToActionResult>(sonuc);
            Assert.Equal("Şifreniz başarıyla güncellendi!", controller.TempData["Basari"]);

            
            var guncelKullanici = context.Kullanicilar.First(k => k.KullaniciAdi == "admin");
            Assert.Equal("yeni456", guncelKullanici.Sifre);
        }
    }
}