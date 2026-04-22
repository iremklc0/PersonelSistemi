namespace PersonelSistemi.Models
{
    public class GridKolonModel
    {
        public string? Baslik { get; set; }
        public string? VeriAlani { get; set; }
        public int? MaxKarakter { get; set; }
        public bool FiltrelenebilirMi { get; set; } = true;
        public bool SiralanabilirMi { get; set; } = false;
    }
}