using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PersonelSistemi.Controllers;
using System.IO;
using System.Text;
using Xunit;

namespace PersonelSistemi.Tests
{
    public class MedyaControllerTests
    {
        
        [Fact]
        public async Task DosyaYukle_BosListe_BadRequestDonmeli()
        {
            var controller = new MedyaController();

            var sonuc = await controller.DosyaYukle(new List<IFormFile>());

            Assert.IsType<BadRequestObjectResult>(sonuc);
        }

       
        [Fact]
        public async Task DosyaYukle_NullListe_BadRequestDonmeli()
        {
            var controller = new MedyaController();

            var sonuc = await controller.DosyaYukle(null);

            Assert.IsType<BadRequestObjectResult>(sonuc);
        }

        
        [Fact]
        public void DosyaSil_BosDosyaAdi_BadRequestDonmeli()
        {
            var controller = new MedyaController();

            var sonuc = controller.DosyaSil("");

            Assert.IsType<BadRequestObjectResult>(sonuc);
        }

        
        [Fact]
        public void DosyaSil_NullDosyaAdi_BadRequestDonmeli()
        {
            var controller = new MedyaController();

            var sonuc = controller.DosyaSil(null);

            Assert.IsType<BadRequestObjectResult>(sonuc);
        }

       
        [Fact]
        public void DosyaSil_OlmayanDosya_BadRequestDonmeli()
        {
            var controller = new MedyaController();

            var sonuc = controller.DosyaSil("kesinlikle_olmayan_dosya_12345.txt");

            Assert.IsType<BadRequestObjectResult>(sonuc);
        }

        
        [Fact]
        public void GaleriYenile_PartialViewDonmeli()
        {
            var controller = new MedyaController();

            var sonuc = controller.GaleriYenile();

            var partial = Assert.IsType<PartialViewResult>(sonuc);
            Assert.Equal("_GaleriPartial", partial.ViewName);
        }
    }
}