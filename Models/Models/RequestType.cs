namespace AspnetCoreMvcFull.Models.Models
{
  public class RequestType
  {
    public int Id { get; set; }
    public string Type { get; set; }

    public ICollection<Request> Requesters { get; set; }
  }
}
