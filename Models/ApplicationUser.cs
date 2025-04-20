using AspnetCoreMvcFull.Models.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspnetCoreMvcFull.Models
{
  public class ApplicationUser : IdentityUser<int>
  {
    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik Numarası 11 haneli olmalıdır.")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "TC Kimlik Numarası yalnızca rakamlardan oluşmalıdır.")]
    public string TcKimlikNo { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(100)]
    public string LastName { get; set; }

    [Required]
    public int UnitId { get; set; }

    [ForeignKey("UnitId")]
    public virtual RequestUnit? Unit { get; set; }
    public bool IsAdmin { get; set; }
  }

}
