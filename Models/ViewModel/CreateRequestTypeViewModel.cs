using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models.ViewModel
{
  public class CreateRequestTypeViewModel
  {
    [Required(ErrorMessage = "Talep tipi gereklidir.")]
    public string Type { get; set; }
  }
}
