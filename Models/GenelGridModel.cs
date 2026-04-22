using System.Collections.Generic;

namespace PersonelSistemi.Models
{
    public class GenelGridModel
    {
        public List<Dictionary<string, object?>> Veriler { get; set; } = new List<Dictionary<string, object?>>();
        public List<GridKolonModel> Kolonlar { get; set; } = new List<GridKolonModel>();
        public List<GridButonModel> Butonlar { get; set; } = new List<GridButonModel>();
        public string ControllerAdi { get; set; } = string.Empty;
        public bool IsFiltered { get; set; } = false;
        public bool IsPaging { get; set; } = false;
        public bool IsSorted { get; set; } = false;
        public int SayfaBasinaKayitSayisi { get; set; } = 10;
        public string SiralamaAlani { get; set; } = string.Empty;
        public string SiralamaCiheti { get; set; } = "asc";
    }
}