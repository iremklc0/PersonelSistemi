using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonelSistemi.Models
{
    [Table("InsaatDurumlari")]
    public class InsaatDurumu
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("durum_adi")]
        public string? DurumAdi { get; set; }

        [Column("renk_kodu")]
        public string? RenkKodu { get; set; }
    }
}