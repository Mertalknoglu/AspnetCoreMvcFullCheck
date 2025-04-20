namespace AspnetCoreMvcFull.Models.Models
{
  public class RequestStatus
  {
    public int Id { get; set; }
    public string Status { get; set; }

    public ICollection<Request> Requesters { get; set; }
  }
}
