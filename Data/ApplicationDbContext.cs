using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Models.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }


  public DbSet<Request> Requests { get; set; }
  public DbSet<RequestStatus> RequestStatuses { get; set; }
  public DbSet<RequestType> RequestTypes { get; set; }
  public DbSet<RequestFilePath> RequestFilePaths { get; set; }
  public DbSet<RequestUnit> RequestUnits { get; set; }
  public DbSet<RequestLog> RequestLogs { get; set; }
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.Entity<ApplicationUser>()
      .HasOne(u => u.Unit)
      .WithMany()
      .HasForeignKey(u => u.UnitId)
      .OnDelete(DeleteBehavior.Restrict);
    // Relationship Configuration
    modelBuilder.Entity<Request>()
        .HasOne(r => r.RequestStatus)
        .WithMany(rs => rs.Requesters)
        .HasForeignKey(r => r.RequestStatusId);

    modelBuilder.Entity<Request>()
        .HasOne(r => r.RequestType)
        .WithMany(rt => rt.Requesters)
        .HasForeignKey(r => r.RequestTypeId);

    modelBuilder.Entity<Request>()
       .HasOne(r => r.RequestUnit)
       .WithMany(ru => ru.Requesters)
       .HasForeignKey(r => r.RequestUnitId);
    modelBuilder.Entity<RequestFilePath>()
        .HasOne(rfp => rfp.Requester)
        .WithMany(r => r.RequestFilePaths)
        .HasForeignKey(rfp => rfp.RequestDetailsId);

    modelBuilder.Entity<RequestFilePath>()
        .HasOne(r => r.Requester)
        .WithMany(p => p.RequestFilePaths)
        .HasForeignKey(r => r.RequestDetailsId)
        .OnDelete(DeleteBehavior.Cascade);
  }

}
