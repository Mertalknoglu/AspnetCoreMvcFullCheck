using AspnetCoreMvcFull.Filters;
using AspnetCoreMvcFull.Models.Models;
using AspnetCoreMvcFull.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Controllers
{
  [AdminOnly]
  public class RequestUnitController : Controller
  {
    private readonly ApplicationDbContext _context;

    public RequestUnitController(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IActionResult> Index()
    {
      var units = await _context.RequestUnits.ToListAsync();
      return View(units);
    }

    // Birim Ekleme İşlemi (Post)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRequestUnitViewModel model)
    {
      if (!ModelState.IsValid)
      {
        // Model geçerli değilse hata mesajlarını göster
        return View(model);
      }

      // Yeni bir RequestUnit objesi oluşturuluyor
      var unit = new RequestUnit
      {
        Unit = model.Unit
      };

      // Veritabanına kaydetme işlemi
      _context.RequestUnits.Add(unit);
      await _context.SaveChangesAsync();

      // Başarı mesajı
      TempData["SuccessMessage"] = "Birim başarıyla eklendi.";
      return RedirectToAction("Index"); // Index sayfasına yönlendirme
    }

    [HttpPost]
    public async Task<IActionResult> Edit(RequestUnit model)
    {
      var unit = await _context.RequestUnits.FindAsync(model.Id);
      if (unit == null)
        return Json(new { success = false, message = "Birim bulunamadı." });

      unit.Unit = model.Unit;
      await _context.SaveChangesAsync();

      return Json(new { success = true, message = "Birim güncellendi." });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
      var unit = await _context.RequestUnits.FindAsync(id);
      if (unit == null)
        return Json(new { success = false });

      _context.RequestUnits.Remove(unit);
      await _context.SaveChangesAsync();

      return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
      var unit = await _context.RequestUnits.FindAsync(id);
      if (unit == null)
        return Json(new { success = false });

      return Json(new { success = true, data = unit });
    }

    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }


  }
}
