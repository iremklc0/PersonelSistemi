namespace PersonelSistemi.Models
{
    public class KonteynerAPersonel
    {
        public int Id { get; set; }

        public int KonteynerId { get; set; }
        public Konteyner Konteyner { get; set; }

        public int PersonelId { get; set; }
        public Personel Personel { get; set; }
    }
}