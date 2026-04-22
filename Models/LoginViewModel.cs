using System.ComponentModel.DataAnnotations;

namespace PersonelSistemi.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Lütfen kullanıcı adınızı giriniz.")]
        public string? KullaniciAdi { get; set; }

        [Required(ErrorMessage = "Lütfen şifrenizi giriniz.")]
        public string? Sifre { get; set; }
    }
}