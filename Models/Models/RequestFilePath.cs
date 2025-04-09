using AspnetCoreMvcFull.Models.Models;

public class RequestFilePath
{
  public int Id { get; set; }
  public int FilePathsId { get; set; }
  public FilePath FilePath { get; set; }

  public int RequestDetailsId { get; set; }
  public Request Requester { get; set; }
}
