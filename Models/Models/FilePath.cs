public class FilePath
{
  public int Id { get; set; }
  public string Filepath { get; set; }

  public ICollection<RequestFilePath> RequestFilePaths { get; set; }
}
