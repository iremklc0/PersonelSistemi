using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OtekSistem.Models;

namespace PersonelSistemi.Models
{
    [Table("InsaatPersonelleri")]
    public class InsaatPersonel
    {
        [Key]
        public int Id { get; set; }

        public int InsaatId { get; set; }
        [ForeignKey("InsaatId")]
        public virtual Insaat? Insaat { get; set; }

        public int PersonelId { get; set; }
        [ForeignKey("PersonelId")]
        public virtual Personel? Personel { get; set; }
    }
}