using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelSistemi.Controllers;
using PersonelSistemi.Models;
using Xunit;

namespace PersonelSistemi.Tests
{
    public class LoginControllerTests
    {
        private AppDbContext SahteVeritabaniOlustur()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        
        [Fact]
        public void Index_IlkAcilis_AdminKullaniciEklenmeli()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new LoginController(context);

            var sonuc = controller.Index();

            Assert.IsType<ViewResult>(sonuc);
            Assert.Single(context.Kullanicilar);
            Assert.Equal("admin", context.Kullanicilar.First().KullaniciAdi);
        }

        
        [Fact]
        public void Index_AdminVarsa_TekrarEklenmemeli()
        {
            var context = SahteVeritabaniOlustur();
            context.Kullanicilar.Add(new Kullanici { KullaniciAdi = "admin", Sifre = "123" });
            context.SaveChanges();

            var controller = new LoginController(context);

            var sonuc = controller.Index();

            Assert.Single(context.Kullanicilar); 
        }

        
        [Fact]
        public void Index_DogruBilgilerleGiris_HomeYonlendirmeli()
        {
            var context = SahteVeritabaniOlustur();
            context.Kullanicilar.Add(new Kullanici { KullaniciAdi = "admin", Sifre = "123" });
            context.SaveChanges();

            var controller = new LoginController(context);
            var model = new LoginViewModel
            {
                KullaniciAdi = "admin",
                Sifre = "123"
            };

            var sonuc = controller.Index(model);

            var yonlendirme = Assert.IsType<RedirectToActionResult>(sonuc);
            Assert.Equal("Index", yonlendirme.ActionName);
            Assert.Equal("Home", yonlendirme.ControllerName);
        }

        
        [Fact]
        public void Index_YanlisSifre_ViewDonmeli()
        {
            var context = SahteVeritabaniOlustur();
            context.Kullanicilar.Add(new Kullanici { KullaniciAdi = "admin", Sifre = "123" });
            context.SaveChanges();

            var controller = new LoginController(context);
            var model = new LoginViewModel
            {
                KullaniciAdi = "admin",
                Sifre = "yanlis"
            };

            var sonuc = controller.Index(model);

            Assert.IsType<ViewResult>(sonuc);
            Assert.False(controller.ModelState.IsValid);
        }

        
        [Fact]
        public void CikisYap_LoginSayfasinaYonlendirmeli()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new LoginController(context);

            var sonuc = controller.CikisYap();

            var yonlendirme = Assert.IsType<RedirectToActionResult>(sonuc);
            Assert.Equal("Index", yonlendirme.ActionName);
            Assert.Equal("Login", yonlendirme.ControllerName);
        }
    }
}