// Models/InsaatBPersonel.cs
using OtekSistem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonelSistemi.Models
{
    [Table("InsaatBPersonelleri")]
    public class InsaatBPersonel
    {
        [Key]
        public int Id { get; set; }

        public int InsaatId { get; set; }
        [ForeignKey("InsaatId")]
        public virtual Insaat? Insaat { get; set; }

        public int BPersonelId { get; set; }
        [ForeignKey("BPersonelId")]
        public virtual BPersonel? BPersonel { get; set; } = null!;
    }
}