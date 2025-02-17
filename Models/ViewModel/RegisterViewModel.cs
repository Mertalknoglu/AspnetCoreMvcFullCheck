using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models
{
  public class RegisterViewModel
  {
    [Required(ErrorMessage = "T.C Kimlik Numarası zorunlu alandır.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik Numarası 11 haneli olmalıdır.")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "TC Kimlik Numarası yalnızca rakamlardan oluşmalıdır.")]
    public string TcKimlikNo { get; set; }

    [Required(ErrorMessage = "İsim  zorunlu alandır.")]
    [StringLength(100)]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Soyisim zorunlu alandır.")]
    [StringLength(100)]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Mail Adresi zorunlu alandır.")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "Telefon Numarası zorunlu alandır.")]
    [DataType(DataType.PhoneNumber)]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Başında 0 olmadan Geçerli bir telefon numarası girin.")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Şifre zorunlu alandır.")]
    [DataType(DataType.Password)]
    [StringLength(11, MinimumLength = 6, ErrorMessage = "Şifre 11 haneli olmalıdır.")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "Şifre bir büyük karakter bir rakam ve bir sembol içermelidir.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Şifre tekrar alanı zorunlu alandır.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
    public string ConfirmPassword { get; set; }
  }
}
