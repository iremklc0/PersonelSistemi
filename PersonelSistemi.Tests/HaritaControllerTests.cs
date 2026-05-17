using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelSistemi.Controllers;
using PersonelSistemi.Models;
using OtekSistem.Models;
using Xunit;

namespace PersonelSistemi.Tests
{
    public class HaritaControllerTests
    {
        private AppDbContext SahteVeritabaniOlustur()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        // Yardimci: Sahte konteyner olustur
        private Konteyner SahteKonteynerOlustur(AppDbContext context, string ad = "Test Konteyner")
        {
            var konteyner = new Konteyner
            {
                Ad = ad,
                Enlem = 39.92,
                Boylam = 32.85,
                KayitTarihi = DateTime.UtcNow
            };
            context.Konteynerler.Add(konteyner);
            context.SaveChanges();
            return konteyner;
        }

        // TEST 1: Index sayfasi ViewResult donmeli
        [Fact]
        public void Index_ViewResultDonmeli()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new HaritaController(context);

            var sonuc = controller.Index();

            Assert.IsType<ViewResult>(sonuc);
        }

        // TEST 2: KonteynerleriGetir bos veritabaninda JsonResult donmeli
        [Fact]
        public void KonteynerleriGetir_BosVeritabani_JsonDonmeli()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new HaritaController(context);

            var sonuc = controller.KonteynerleriGetir();

            Assert.IsType<JsonResult>(sonuc);
        }

        // TEST 3: KonteynerleriGetir konteynerler varsa veri donmeli
        [Fact]
        public void KonteynerleriGetir_KonteynerVarsa_VeriDonmeli()
        {
            var context = SahteVeritabaniOlustur();
            SahteKonteynerOlustur(context, "Otek Konteyner");
            SahteKonteynerOlustur(context, "Sahantiye 1");

            var controller = new HaritaController(context);
            var sonuc = controller.KonteynerleriGetir();

            Assert.IsType<JsonResult>(sonuc);
            Assert.Equal(2, context.Konteynerler.Count());
        }

        // TEST 4: KonteynerSil var olan konteyneri silmeli
        [Fact]
        public void KonteynerSil_VarOlanKonteyner_BasariliSilinmeli()
        {
            var context = SahteVeritabaniOlustur();
            var konteyner = SahteKonteynerOlustur(context);

            var controller = new HaritaController(context);
            var sonuc = controller.KonteynerSil(konteyner.Id);

            Assert.IsType<JsonResult>(sonuc);
            Assert.Empty(context.Konteynerler);
        }

        // TEST 5: KonteynerSil olmayan id basarisiz olmali
        [Fact]
        public void KonteynerSil_OlmayanId_BasarisizOlmali()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new HaritaController(context);

            var sonuc = controller.KonteynerSil(99999);

            Assert.IsType<JsonResult>(sonuc);
        }

        // TEST 6: KonteynerPersonelEkle gecerli A personeli eklemeli
        [Fact]
        public void KonteynerPersonelEkle_GecerliAPersonel_BasariliEklenmeli()
        {
            var context = SahteVeritabaniOlustur();
            var konteyner = SahteKonteynerOlustur(context);

            var personel = new Personel
            {
                adi = "Ali",
                soyadi = "Veli",
                cinsiyet = "Erkek",
                tc = "12345678901",
                personel_id = "K001"
            };
            context.Personeller.Add(personel);
            context.SaveChanges();

            var controller = new HaritaController(context);
            var sonuc = controller.KonteynerPersonelEkle(konteyner.Id, personel.objectid, "A");

            Assert.IsType<JsonResult>(sonuc);
            Assert.Single(context.KonteynerAPersoneller);
        }

        // TEST 7: KonteynerPersonelEkle olmayan konteyner hata vermeli
        [Fact]
        public void KonteynerPersonelEkle_OlmayanKonteyner_HataVermeli()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new HaritaController(context);

            var sonuc = controller.KonteynerPersonelEkle(99999, 1, "A");

            Assert.IsType<JsonResult>(sonuc);
            Assert.Empty(context.KonteynerAPersoneller);
        }

        // TEST 8: KonteynerPersonelEkle ayni personeli iki kez eklemeye calisma
        [Fact]
        public void KonteynerPersonelEkle_AyniPersonelTekrar_HataVermeli()
        {
            var context = SahteVeritabaniOlustur();
            var konteyner = SahteKonteynerOlustur(context);

            var personel = new Personel
            {
                adi = "Test",
                soyadi = "Kisi",
                cinsiyet = "Kadın",
                tc = "11122233344",
                personel_id = "K002"
            };
            context.Personeller.Add(personel);
            context.SaveChanges();

            var controller = new HaritaController(context);
            controller.KonteynerPersonelEkle(konteyner.Id, personel.objectid, "A"); // İlk
            var sonuc = controller.KonteynerPersonelEkle(konteyner.Id, personel.objectid, "A"); // İkinci

            Assert.IsType<JsonResult>(sonuc);
            Assert.Single(context.KonteynerAPersoneller); // Hala 1 olmali
        }

        // TEST 9: KonteynerPersonelCikar var olan personeli cikarmali
        [Fact]
        public void KonteynerPersonelCikar_VarOlanPersonel_BasariliCikarmali()
        {
            var context = SahteVeritabaniOlustur();
            var konteyner = SahteKonteynerOlustur(context);

            var personel = new Personel
            {
                adi = "Test",
                soyadi = "Kisi",
                cinsiyet = "Erkek",
                tc = "55566677788",
                personel_id = "K003"
            };
            context.Personeller.Add(personel);
            context.KonteynerAPersoneller.Add(new KonteynerAPersonel { KonteynerId = konteyner.Id, PersonelId = personel.objectid });
            context.SaveChanges();

            var controller = new HaritaController(context);
            var sonuc = controller.KonteynerPersonelCikar(konteyner.Id, personel.objectid, "A");

            Assert.IsType<JsonResult>(sonuc);
            Assert.Empty(context.KonteynerAPersoneller);
        }
    }
}