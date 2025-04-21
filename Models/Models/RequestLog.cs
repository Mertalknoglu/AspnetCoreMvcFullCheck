using AspnetCoreMvcFull.Models.Models;

public class RequestLog
{
  public int Id { get; set; }
  public int RequestId { get; set; }
  public string ActionType { get; set; } // "Durum Güncellendi", "Tip Değiştirildi" gibi
  public string Description { get; set; } // Eski-Değişen değerleri açıklamak için
  public string ChangedBy { get; set; } // Kullanıcı Adı veya ID
  public DateTime ChangedAt { get; set; }

  public Request Request { get; set; }
}
