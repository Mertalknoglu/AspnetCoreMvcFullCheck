using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvcFull.Models.ViewModel
{
  public class CreateUnitViewModel
  {
    public int Id { get; set; }

    [Required(ErrorMessage = "Birim adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Birim adı en fazla 100 karakter olabilir.")]
    public string Unit { get; set; }
  }
}
