public class RequestStatus
{
  public int Id { get; set; }
  public string Status { get; set; }

  public ICollection<Requester> Requesters { get; set; }
}
