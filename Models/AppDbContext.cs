using Microsoft.EntityFrameworkCore;
using OtekSistem.Models; 

namespace PersonelSistemi.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Personel> Personeller { get; set; }
        public DbSet<BPersonel> BPersoneller { get; set; }
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Insaat> Insaatlar { get; set; }
        public DbSet<InsaatDurumu> InsaatDurumlari { get; set; }
        public DbSet<InsaatPersonel> InsaatPersonelleri { get; set; }
        public DbSet<InsaatBPersonel> InsaatBPersonelleri { get; set; }

        
        public DbSet<Konteyner> Konteynerler { get; set; }
        public DbSet<KonteynerAPersonel> KonteynerAPersoneller { get; set; }
        public DbSet<KonteynerBPersonel> KonteynerBPersoneller { get; set; }
    }
}