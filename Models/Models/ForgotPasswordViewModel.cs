using System.ComponentModel.DataAnnotations;

public class ForgotPasswordViewModel
{
  [Required(ErrorMessage = "E-posta adresi zorunludur.")]
  [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
  public string Email { get; set; }
}
