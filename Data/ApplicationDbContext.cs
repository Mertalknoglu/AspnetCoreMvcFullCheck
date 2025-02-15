using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


  public DbSet<Requester> Requesters { get; set; }
  public DbSet<RequestStatus> RequestStatuses { get; set; }
  public DbSet<RequestType> RequestTypes { get; set; }
  public DbSet<FilePath> FilePaths { get; set; }
  public DbSet<RequestFilePath> RequestFilePaths { get; set; }

  // OnModelCreating metodu ile ilişkileri ve diğer ayarları yapabilirsiniz.
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Relationship Configuration
    modelBuilder.Entity<Requester>()
        .HasOne(r => r.RequestStatus)
        .WithMany(rs => rs.Requesters)
        .HasForeignKey(r => r.RequestStatusId);

    modelBuilder.Entity<Requester>()
        .HasOne(r => r.RequestType)
        .WithMany(rt => rt.Requesters)
        .HasForeignKey(r => r.RequestTypeId);

    modelBuilder.Entity<RequestFilePath>()
        .HasOne(rfp => rfp.Requester)
        .WithMany(r => r.RequestFilePaths)
        .HasForeignKey(rfp => rfp.RequestDetailsId);

    modelBuilder.Entity<RequestFilePath>()
        .HasOne(rfp => rfp.FilePath)
        .WithMany(fp => fp.RequestFilePaths)
        .HasForeignKey(rfp => rfp.FilePathsId);
  }

}
