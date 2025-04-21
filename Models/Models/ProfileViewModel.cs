namespace AspnetCoreMvcFull.Models.Models
{
  public class ProfileViewModel
  {
    // Mevcut Bilgiler
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }

    // Yeni Eklenenler
    public string PhoneNumber { get; set; }
    public string UnitName { get; set; }
    public bool IsAdmin { get; set; }
    public string? ProfilePicture { get; set; }
    public IFormFile? UploadedPhoto { get; set; }

    public string Yetki => IsAdmin ? "Admin" : "Kullanıcı";

    // Şifre alanları
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
  }
}
