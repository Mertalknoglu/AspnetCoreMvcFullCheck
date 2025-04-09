
namespace AspnetCoreMvcFull.Models.Models
{
  public class Request
  {
    public int Id { get; set; }
    public string Tckn { get; set; }
    public string FirstName { get; set; }
    public string Surname { get; set; }
    public string TelNo { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }

    public int RequestStatusId { get; set; }
    public RequestStatus RequestStatus { get; set; }

    public int RequestTypeId { get; set; }
    public RequestType RequestType { get; set; }

    public int RequestUnitId { get; set; }
    public RequestUnit RequestUnit { get; set; }

    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public ICollection<RequestFilePath> RequestFilePaths { get; set; }
  }
}
