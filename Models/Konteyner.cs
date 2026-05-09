using System.Collections.Generic;

namespace PersonelSistemi.Models
{
    public class Konteyner
    {
        public int Id { get; set; }
        public string Ad { get; set; } // Örn: "A Blok Konteyner", "Şantiye Evleri"

        // Haritadaki konumu için
        public double Enlem { get; set; }
        public double Boylam { get; set; }

        // Konteynerde kalacak personellerin listeleri
        public List<KonteynerAPersonel> KonteynerAPersoneller { get; set; } = new List<KonteynerAPersonel>();
        public List<KonteynerBPersonel> KonteynerBPersoneller { get; set; } = new List<KonteynerBPersonel>();
        public DateTime KayitTarihi { get; set; }
    }
}