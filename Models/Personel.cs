using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace PersonelSistemi.Models
{
    [Table("Personeller")]
    public class Personel
    {
        [Key]
        [Column("objectid")]
        public int objectid { get; set; }

        [Required(ErrorMessage = "Sicil No alanı zorunludur!")]
        [Column("personel_id")]
        public string? personel_id { get; set; }

        [Required(ErrorMessage = "Ad alani zorunludur!")]
        [Column("adi")]
        public string? adi { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur!")]
        [Column("soyadi")]
        public string? soyadi { get; set; }

        [Required(ErrorMessage = "TC Kimlik alanı zorunludur!")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik numarası tam 11 haneli olmalıdır!")]
        [Column("tc")]
        public string? tc { get; set; }
        
        public virtual ICollection<InsaatPersonel> InsaatPersonelleri { get; set; } = new List<InsaatPersonel>();
    }
}