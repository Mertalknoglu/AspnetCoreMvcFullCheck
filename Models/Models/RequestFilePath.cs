public class RequestFilePath
{
  public int Id { get; set; }
  public int FilePathsId { get; set; }
  public FilePath FilePath { get; set; }

  public int RequestDetailsId { get; set; }
  public Requester Requester { get; set; }
}
