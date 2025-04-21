using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models.ViewModel
{
  public class UpdateUserViewModel
  {
    public int Id { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    public string? TcKimlikNo { get; set; }

    public int UnitId { get; set; }

    public bool IsAdmin { get; set; }

    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Şifreler uyuşmuyor")]
    public string? ConfirmPassword { get; set; }
  }
}
