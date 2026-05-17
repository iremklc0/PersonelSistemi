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

        // TEST 9: PersonelEkle gecerli A personeli eklemeli
        [Fact]
        public void PersonelEkle_GecerliAPersonel_BasariliEklenmeli()
        {
            var context = SahteVeritabaniOlustur();
            var insaat = SahteInsaatOlustur(context);

            var personel = new Personel
            {
                adi = "Ali",
                soyadi = "Veli",
                cinsiyet = "Erkek",
                tc = "12345678901",
                personel_id = "P001"
            };
            context.Personeller.Add(personel);
            context.SaveChanges();

            var controller = new InsaatController(context);
            var sonuc = controller.PersonelEkle(insaat.Id, personel.objectid, "A");

            Assert.IsType<JsonResult>(sonuc);
            Assert.Single(context.InsaatPersonelleri);
        }

        // TEST 10: PersonelEkle olmayan insaat hata vermeli
        [Fact]
        public void PersonelEkle_OlmayanInsaat_HataVermeli()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new InsaatController(context);

            var sonuc = controller.PersonelEkle(99999, 1, "A");

            Assert.IsType<JsonResult>(sonuc);
            Assert.Empty(context.InsaatPersonelleri);
        }

        // TEST 11: PersonelEkle ayni personeli iki kez eklemeye calisma
        [Fact]
        public void PersonelEkle_AyniPersonelTekrar_HataVermeli()
        {
            var context = SahteVeritabaniOlustur();
            var insaat = SahteInsaatOlustur(context);

            var personel = new Personel
            {
                adi = "Test",
                soyadi = "Kisi",
                cinsiyet = "Kadın",
                tc = "11122233344",
                personel_id = "P002"
            };
            context.Personeller.Add(personel);
            context.SaveChanges();

            var controller = new InsaatController(context);
            controller.PersonelEkle(insaat.Id, personel.objectid, "A"); // İlk ekleme
            var sonuc = controller.PersonelEkle(insaat.Id, personel.objectid, "A"); // İkinci ekleme

            Assert.IsType<JsonResult>(sonuc);
            Assert.Single(context.InsaatPersonelleri); // Hala 1 tane olmali, 2 olmamali
        }

        // TEST 12: PersonelCikar var olan personeli cikarmali
        [Fact]
        public void PersonelCikar_VarOlanPersonel_BasariliCikarmali()
        {
            var context = SahteVeritabaniOlustur();
            var insaat = SahteInsaatOlustur(context);

            var personel = new Personel
            {
                adi = "Test",
                soyadi = "Kisi",
                cinsiyet = "Erkek",
                tc = "55566677788",
                personel_id = "P003"
            };
            context.Personeller.Add(personel);
            context.InsaatPersonelleri.Add(new InsaatPersonel { InsaatId = insaat.Id, PersonelId = personel.objectid });
            context.SaveChanges();

            var controller = new InsaatController(context);
            var sonuc = controller.PersonelCikar(insaat.Id, personel.objectid, "A");

            Assert.IsType<JsonResult>(sonuc);
            Assert.Empty(context.InsaatPersonelleri);
        }

        // TEST 13: InsaatSil var olan insaati silmeli
        [Fact]
        public void InsaatSil_VarOlanInsaat_BasariliSilinmeli()
        {
            var context = SahteVeritabaniOlustur();
            var insaat = SahteInsaatOlustur(context);

            var controller = new InsaatController(context);
            var sonuc = controller.InsaatSil(insaat.Id);

            Assert.IsType<JsonResult>(sonuc);
            Assert.Empty(context.Insaatlar);
        }

        // TEST 14: InsaatSil olmayan id basarisiz olmali
        [Fact]
        public void InsaatSil_OlmayanId_BasarisizOlmali()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new InsaatController(context);

            var sonuc = controller.InsaatSil(99999);

            Assert.IsType<JsonResult>(sonuc);
        }

        // TEST 15: InsaatEkle gecerli veriyle basarili eklenmeli
        [Fact]
        public void InsaatEkle_GecerliVeri_BasariliEklenmeli()
        {
            var context = SahteVeritabaniOlustur();
            var controller = new InsaatController(context);

            var yeniInsaat = new OtekSistem.Models.Insaat
            {
                InsaatAdi = "Yeni Bina",
                InsaatTuru = "Konut",
                Aciklama = "Test bina",
                KoordinatX = 32.85,
                KoordinatY = 39.92,
                DurumId = 1,
                TamamlanmaYuzdesi = 0
            };

            var sonuc = controller.InsaatEkle(yeniInsaat, new List<int>(), new List<int>());

            Assert.IsType<JsonResult>(sonuc);
            Assert.Single(context.Insaatlar);
            Assert.Equal("Yeni Bina", context.Insaatlar.First().InsaatAdi);
        }
    }
}