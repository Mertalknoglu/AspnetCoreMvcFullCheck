using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models
{
  public class LoginViewModel
  {
    [Required(ErrorMessage = "T.C Kimlik Numarası zorunlu alandır.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik Numarası 11 haneli olmalıdır.")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "TC Kimlik Numarası yalnızca rakamlardan oluşmalıdır.")]
    public string TcKimlikNo { get; set; }

    [Required(ErrorMessage = "Şifre zorunlu alandır.")]
    [DataType(DataType.Password)]

    public string Password { get; set; }

    public bool RememberMe { get; set; }
  }
}
