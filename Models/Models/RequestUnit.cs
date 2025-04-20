namespace AspnetCoreMvcFull.Models.Models
{
  public class RequestUnit
  {
    public int Id { get; set; }
    public string Unit { get; set; }
    public ICollection<Request> Requesters { get; set; }
  }
}
