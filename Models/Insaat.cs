using PersonelSistemi.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
namespace OtekSistem.Models
{
    [Table("Insaatlar")]
    public class Insaat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? InsaatAdi { get; set; }

        public string? InsaatTuru { get; set; }

        public double KoordinatX { get; set; }
        public double KoordinatY { get; set; }

        public int DurumId { get; set; }

        [ForeignKey("DurumId")]
        public virtual InsaatDurumu? InsaatDurumu { get; set; }


        public string? Aciklama { get; set; }

        public DateTime KayitTarihi { get; set; } = DateTime.Now;
     
        public virtual ICollection<PersonelSistemi.Models.InsaatPersonel> InsaatPersonelleri { get; set; } = new List<PersonelSistemi.Models.InsaatPersonel>();

    }
}