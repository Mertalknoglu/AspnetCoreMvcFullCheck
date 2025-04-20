using AspnetCoreMvcFull.Models.Models;
using AspnetCoreMvcFull.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Controllers
{
  public class RequestTypeController : Controller
  {
    private readonly ApplicationDbContext _context;

    public RequestTypeController(ApplicationDbContext context)
    {
      _context = context;
    }

    // Talep Tipi Listesi Sayfası (GET)
    public async Task<IActionResult> Index()
    {
      var requestTypes = await _context.RequestTypes.ToListAsync();
      return View(requestTypes);
    }

    // Talep Tipi Ekleme Sayfası (GET)
    [HttpGet]
    public IActionResult Create()
    {
      return View();
    }

    // Talep Tipi Ekleme İşlemi (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRequestTypeViewModel model)
    {
      if (!ModelState.IsValid)
      {
        // Eğer model geçerli değilse, formu yeniden göster
        return View(model);
      }

      // Yeni bir RequestType objesi oluşturuluyor
      var requestType = new RequestType
      {
        Type = model.Type
      };

      // Veritabanına kaydetme işlemi
      _context.RequestTypes.Add(requestType);
      await _context.SaveChangesAsync();

      // Başarı mesajı
      TempData["SuccessMessage"] = "Talep tipi başarıyla oluşturuldu.";
      return RedirectToAction("Index"); // Index sayfasına yönlendirme
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CustomEdit(RequestTypeViewModel model)
    {
      var requestType = await _context.RequestTypes.FindAsync(model.Id);
      if (requestType == null)
        return Json(new { success = false, message = "Talep tipi bulunamadı." });

      requestType.Type = model.Type;
      await _context.SaveChangesAsync();

      return Json(new { success = true, message = "Talep tipi başarıyla güncellendi." });
    }




    // Talep Tipi Silme (POST)
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
      var requestType = await _context.RequestTypes.FindAsync(id);
      if (requestType == null)
        return Json(new { success = false, message = "Talep tipi bulunamadı." });

      _context.RequestTypes.Remove(requestType);
      await _context.SaveChangesAsync();

      return Json(new { success = true, message = "Talep tipi başarıyla silindi." });
    }

    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
      var requestType = await _context.RequestTypes.FindAsync(id);
      if (requestType == null)
        return Json(new { success = false });

      return Json(new { success = true, data = requestType });
    }

  }
}
