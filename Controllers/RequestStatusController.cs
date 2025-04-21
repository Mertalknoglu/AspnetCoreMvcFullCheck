using AspnetCoreMvcFull.Filters;
using AspnetCoreMvcFull.Models.Models;
using AspnetCoreMvcFull.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Controllers
{
  [AdminOnly]
  public class RequestStatusController : Controller
  {
    private readonly ApplicationDbContext _context;

    public RequestStatusController(ApplicationDbContext context)
    {
      _context = context;
    }

    // Talep Durumu Listesi Sayfası (GET)
    public async Task<IActionResult> Index()
    {
      var requestStatuses = await _context.RequestStatuses.ToListAsync();
      return View(requestStatuses);
    }

    // Talep Durumu Ekleme Sayfası (GET)
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }

    // Talep Durumu Ekleme İşlemi (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRequestStatusViewModel model)
    {
      if (!ModelState.IsValid)
      {
        // Eğer model geçerli değilse, formu yeniden göster
        return View(model);
      }

      // Yeni bir RequestStatus objesi oluşturuluyor
      var requestStatus = new RequestStatus
      {
        Status = model.Status
      };

      // Veritabanına kaydetme işlemi
      _context.RequestStatuses.Add(requestStatus);
      await _context.SaveChangesAsync();

      // Başarı mesajı
      TempData["SuccessMessage"] = "Talep durumu başarıyla oluşturuldu.";
      return RedirectToAction("Index"); // Index sayfasına yönlendirme
    }

    // Talep Durumu Güncelleme Sayfası (GET)
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
      var requestStatus = await _context.RequestStatuses.FindAsync(id);
      if (requestStatus == null)
        return NotFound();

      var model = new CreateRequestStatusViewModel
      {
        Status = requestStatus.Status
      };

      return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [FromForm] CreateRequestStatusViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return Json(new { success = false, message = "Geçersiz veri." });
      }

      var requestStatus = await _context.RequestStatuses.FindAsync(id);
      if (requestStatus == null)
      {
        return Json(new { success = false, message = "Durum bulunamadı." });
      }

      requestStatus.Status = model.Status;
      await _context.SaveChangesAsync();

      return Json(new { success = true, message = "Durum başarıyla güncellendi." });
    }


    // Talep Durumu Silme (POST)
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
      var requestStatus = await _context.RequestStatuses.FindAsync(id);
      if (requestStatus == null)
        return Json(new { success = false, message = "Talep durumu bulunamadı." });

      _context.RequestStatuses.Remove(requestStatus);
      await _context.SaveChangesAsync();

      return Json(new { success = true, message = "Talep durumu başarıyla silindi." });
    }

    // Talep Durumu Bilgilerini Getirme (GET)
    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
      var requestStatus = await _context.RequestStatuses.FindAsync(id);
      if (requestStatus == null)
        return Json(new { success = false });

      return Json(new { success = true, data = requestStatus });
    }
  }
}
