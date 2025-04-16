using AspnetCoreMvcFull.Models.Models;

public class RequestFilePath
{
  public int Id { get; set; }
  public string FilePaths { get; set; }

  public int RequestDetailsId { get; set; }
  public Request Requester { get; set; }
}
