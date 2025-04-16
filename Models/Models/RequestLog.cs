public class RequestLog
{
  public int Id { get; set; }
  public int RequestId { get; set; }
  public int UserId { get; set; }
  public string Status { get; set; }
  public string Type { get; set; }
  public string Unit { get; set; }
  public string Description { get; set; }
  public DateTime ChangedAt { get; set; }
}
