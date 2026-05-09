

namespace PersonelSistemi.Models
{
    public class KonteynerBPersonel
    {
        public int Id { get; set; }

        public int KonteynerId { get; set; }
        public Konteyner Konteyner { get; set; }

        public int BPersonelId { get; set; }
        public BPersonel BPersonel { get; set; }
    }
}