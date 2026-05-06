using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace PersonelSistemi.Models
{
    [Table("b_personelleri")]
    public class BPersonel
    {
        [Key]
        [Column("objectid")]
        public int objectid { get; set; }

        [Column("sicil_no")]
        public string? sicil_no { get; set; }

        [Column("adi")]
        public string? adi { get; set; }

        [Column("soyadi")]
        public string? soyadi { get; set; }

        [Column("tc")]
        public string? tc { get; set; }

        public virtual ICollection<InsaatBPersonel> InsaatBPersonelleri { get; set; } = new List<InsaatBPersonel>();  // YENİ
    }
}