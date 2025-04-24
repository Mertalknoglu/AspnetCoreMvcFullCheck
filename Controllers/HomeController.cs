using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Controllers
{
  [Authorize]
  public class HomeController : Controller
  {
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IActionResult> Index()
    {
      var requests = await _context.Requests
          .Include(r => r.RequestStatus)
          .Include(r => r.RequestUnit)
          .Include(r => r.RequestType)
          .Where(r => !r.IsDeleted)
          .ToListAsync();

      ViewBag.TotalRequestCount = requests.Count;
      ViewBag.CompletedCount = requests.Count(r => r.RequestStatus.Status == "Sonuçlandı");
      ViewBag.PendingCount = requests.Count(r => r.RequestStatus.Status == "Bekliyor");
      ViewBag.CancelledCount = requests.Count(r => r.RequestStatus.Status == "İptal");

      var now = DateTime.Now;
      var startOfDay = now.Date;
      var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek + 1); // Pazartesi
      var startOfMonth = new DateTime(now.Year, now.Month, 1);

      ViewBag.DailyData = await GetStatusCounts(startOfDay);
      ViewBag.WeeklyData = await GetStatusCounts(startOfWeek);
      ViewBag.MonthlyData = await GetStatusCounts(startOfMonth);

      return View();
    }

    private async Task<object> GetStatusCounts(DateTime startDate)
    {
      var results = await _context.Requests
          .Include(r => r.RequestStatus)
          .Where(r => !r.IsDeleted && r.Date >= startDate)
          .GroupBy(r => r.RequestStatus.Status)
          .Select(g => new
          {
            Status = g.Key,
            Count = g.Count()
          })
          .ToListAsync();

      int completed = results.FirstOrDefault(r => r.Status == "Sonuçlandı")?.Count ?? 0;
      int pending = results.FirstOrDefault(r => r.Status == "Bekliyor")?.Count ?? 0;
      int cancelled = results.FirstOrDefault(r => r.Status == "İptal")?.Count ?? 0;
      int total = completed + pending + cancelled;

      return new
      {
        Completed = completed,
        Pending = pending,
        Cancelled = cancelled,
        Total = total
      };
    }

    [HttpGet]
    public async Task<IActionResult> GetChartData(string range)
    {
      DateTime startDate = DateTime.Today;

      switch (range.ToLower())
      {
        case "weekly":
          startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1); break;
        case "monthly":
          startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); break;
        case "daily":
        default:
          startDate = DateTime.Today; break;
      }

      var data = await GetStatusCounts(startDate);
      return Json(data);
    }
  }
}
