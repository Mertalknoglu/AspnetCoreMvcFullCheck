
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
    public string? Response { get; set; }
    public DateTime Date { get; set; }

    public int RequestStatusId { get; set; }
    public RequestStatus RequestStatus { get; set; }

    public int RequestTypeId { get; set; }
    public RequestType RequestType { get; set; }

    public int RequestUnitId { get; set; }
    public RequestUnit RequestUnit { get; set; }

    public int UserId { get; set; }
    public ApplicationUser User { get; set; }

    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; } = false;


    public ICollection<RequestFilePath> RequestFilePaths { get; set; }
  }
}
