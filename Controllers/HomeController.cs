using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AspnetCoreMvcFull.Models;
using AspnetCoreMvcFull.Models.Models;

namespace AspnetCoreMvcFull.Controllers
{
  [Authorize]
  public class HomeController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
      _context = context;
      _userManager = userManager;
    }

    // ───────────────────────────────────────────────────────────
    // Dashboard – sayfa, segment bar, dağılımlar ve carousel
    // ───────────────────────────────────────────────────────────
    public async Task<IActionResult> Dashboard(int? page)
    {
      var user = await _userManager.GetUserAsync(User);
      bool isAdmin = user.IsAdmin;

      // 1️⃣ Talep statüleri (segment bar için)
      var statuses = await _context.RequestStatuses
          .OrderBy(s => s.Id)
          .Select(s => s.Status)
          .ToListAsync();

      // 2️⃣ Toplam talep sayısı
      int totalCount = isAdmin
          ? await _context.Requests.CountAsync(r => !r.IsDeleted)
          : await _context.Requests.CountAsync(r => !r.IsDeleted && r.RequestUnitId == user.UnitId);

      // 3️⃣ “İsteklerim” – tüm kayıtlar
      var allReqs = await _context.Requests
          .Where(r => !r.IsDeleted && r.UserId == user.Id)
          .Include(r => r.RequestStatus)
          .Include(r => r.RequestType)
          .Include(r => r.RequestUnit)
          .OrderByDescending(r => r.Date)
          .ToListAsync();

      // 4️⃣ Sayfalama
      int pageSize = 5;
      int pageNumber = page ?? 1;
      var pagedReqs = allReqs
          .Skip((pageNumber - 1) * pageSize)
          .Take(pageSize)
          .ToList();

      // 5️⃣ ViewBag’e aktar
      ViewBag.IsAdmin = isAdmin;
      ViewBag.TotalCount = totalCount;
      ViewBag.Statuses = statuses;
      ViewBag.MyRequests = pagedReqs;
      ViewBag.Page = pageNumber;
      ViewBag.PageSize = pageSize;
      ViewBag.TotalRequests = allReqs.Count;

      return View();
    }

    // ───────────────────────────────────────────────────────────
    // Segment bar için JSON (Günlük/Haftalık/Aylık)
    // ───────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<JsonResult> GetChartData(string range)
    {
      var user = await _userManager.GetUserAsync(User);
      bool isAdmin = user.IsAdmin;

      DateTime from = range.ToLower() switch
      {
        "weekly" => DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1),
        "monthly" => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
        _ => DateTime.Today
      };

      IQueryable<Request> q = _context.Requests
          .Where(r => !r.IsDeleted && r.Date >= from)
          .Include(r => r.RequestStatus);

      if (!isAdmin)
        q = q.Where(r => r.RequestUnitId == user.UnitId);

      var raw = await q
          .GroupBy(r => r.RequestStatus.Status)
          .Select(g => new { Status = g.Key, Count = g.Count() })
          .ToListAsync();

      var dict = raw.ToDictionary(x => x.Status, x => x.Count);
      return Json(dict);
    }

    // ───────────────────────────────────────────────────────────
    // Birim Dağılımı
    // ───────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<JsonResult> GetUnitDistribution(string range)
    {
      var user = await _userManager.GetUserAsync(User);
      bool isAdmin = user.IsAdmin;

      DateTime from = range.ToLower() switch
      {
        "weekly" => DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1),
        "monthly" => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
        _ => DateTime.Today
      };

      IQueryable<Request> q = _context.Requests
          .Where(r => !r.IsDeleted && r.Date >= from)
          .Include(r => r.RequestUnit);

      if (!isAdmin)
        q = q.Where(r => r.RequestUnitId == user.UnitId);

      var raw = await q
          .GroupBy(r => r.RequestUnit.Unit)
          .Select(g => new { Key = g.Key, Count = g.Count() })
          .ToListAsync();

      return Json(raw.ToDictionary(x => x.Key, x => x.Count));
    }

    // ───────────────────────────────────────────────────────────
    // Durum Dağılımı
    // ───────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<JsonResult> GetStatusDistribution(string range)
    {
      var user = await _userManager.GetUserAsync(User);
      bool isAdmin = user.IsAdmin;

      DateTime from = range.ToLower() switch
      {
        "weekly" => DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1),
        "monthly" => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
        _ => DateTime.Today
      };

      IQueryable<Request> q = _context.Requests
          .Where(r => !r.IsDeleted && r.Date >= from)
          .Include(r => r.RequestStatus);

      if (!isAdmin)
        q = q.Where(r => r.RequestUnitId == user.UnitId);

      var raw = await q
          .GroupBy(r => r.RequestStatus.Status)
          .Select(g => new { Key = g.Key, Count = g.Count() })
          .ToListAsync();

      return Json(raw.ToDictionary(x => x.Key, x => x.Count));
    }

    // ───────────────────────────────────────────────────────────
    // Tip Dağılımı
    // ───────────────────────────────────────────────────────────
    [HttpGet]
    public async Task<JsonResult> GetTypeDistribution(string range)
    {
      var user = await _userManager.GetUserAsync(User);
      bool isAdmin = user.IsAdmin;

      DateTime from = range.ToLower() switch
      {
        "weekly" => DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1),
        "monthly" => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
        _ => DateTime.Today
      };

      IQueryable<Request> q = _context.Requests
          .Where(r => !r.IsDeleted && r.Date >= from)
          .Include(r => r.RequestType);

      if (!isAdmin)
        q = q.Where(r => r.RequestUnitId == user.UnitId);

      var raw = await q
          .GroupBy(r => r.RequestType.Type)
          .Select(g => new { Key = g.Key, Count = g.Count() })
          .ToListAsync();

      return Json(raw.ToDictionary(x => x.Key, x => x.Count));
    }
  }
}
