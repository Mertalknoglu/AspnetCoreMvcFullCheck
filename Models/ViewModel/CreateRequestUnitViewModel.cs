using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models.ViewModel
{
  public class CreateRequestUnitViewModel
  {
    [Required(ErrorMessage = "Birim adı gereklidir.")]
    public string Unit { get; set; }
  }
}
