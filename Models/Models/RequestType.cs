public class RequestType
{
  public int Id { get; set; }
  public string Type { get; set; }

  public ICollection<Requester> Requesters { get; set; }
}
