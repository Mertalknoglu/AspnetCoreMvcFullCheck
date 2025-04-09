using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Web;
using AspnetCoreMvcFull.Models.Models;
[Authorize]
public class RequesterController : Controller
{
  private readonly ApplicationDbContext _context;

  public RequesterController(ApplicationDbContext context)
  {
    _context = context;
  }

  // GET: Requester
  public async Task<IActionResult> Index()
  {
    var requesters = await _context.Requests
        .Include(r => r.RequestStatus)
        .Include(r => r.RequestType)
        .ToListAsync();
    return View(requesters);
  }

  // GET: Requester/Create
  public IActionResult Create()
  {
    // RequestStatuses ve RequestTypes listesini ViewData yerine ViewBag üzerinden gönderiyoruz.
    ViewBag.RequestStatusList = new SelectList(_context.RequestStatuses, "Id", "Status");
    ViewBag.RequestTypeList = new SelectList(_context.RequestTypes, "Id", "Type");

    return View();
  }


  // POST: Requester/Create
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create([Bind("Tckn,FirstName,Surname,TelNo,Email,Address,Description,Date,RequestStatusId,RequestTypeId")] Request requester, IFormFile[] files)
  {
    var errors = ValidateRequester(requester);

    // Eğer hata varsa, hata mesajlarını ViewBag'e ekle
    if (errors.Any())
    {
      ViewBag.Errors = errors;
      ViewBag.RequestStatusList = new SelectList(_context.RequestStatuses, "Id", "Status");
      ViewBag.RequestTypeList = new SelectList(_context.RequestTypes, "Id", "Type");
      return View(requester); // Formu tekrar yükle ve hataları göster
    }
    requester.Date = DateTime.Now;
    // Dosya yükleme kısmı
    if (files != null && files.Length > 0)
    {
      var filePaths = new List<FilePath>();

      foreach (var file in files)
      {
        if (file.Length > 0)
        {
          // Dosyayı kaydetme
          var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", file.FileName);

          using (var stream = new FileStream(filePath, FileMode.Create))
          {
            await file.CopyToAsync(stream);
          }

          // Dosya yolunu listeye ekliyoruz
          filePaths.Add(new FilePath { Filepath = "/uploads/" + file.FileName });
        }
      }

      // Dosyaları RequestFilePath tablosuna ekleme
      foreach (var filePath in filePaths)
      {
        var requestFilePath = new RequestFilePath
        {
          FilePath = filePath,
          Requester = requester
        };
        _context.Add(requestFilePath);
      }
    }

    // Requester verisini ekliyoruz
    _context.Add(requester);
    await _context.SaveChangesAsync();
    TempData["SuccessMessage"] = HttpUtility.HtmlEncode("Kayıt başarıyla oluşturuldu!");
    return RedirectToAction(nameof(Index));
  }

  // GET: Requester/Edit/{id}
  public async Task<IActionResult> Edit(int id)
  {
    var requester = await _context.Requests.FindAsync(id);
    if (requester == null)
    {
      return NotFound();
    }

    ViewBag.RequestStatusList = new SelectList(_context.RequestStatuses, "Id", "Status", requester.RequestStatusId);
    ViewBag.RequestTypeList = new SelectList(_context.RequestTypes, "Id", "Type", requester.RequestTypeId);

    return View(requester);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(int id, [Bind("Id,Tckn,FirstName,Surname,TelNo,Email,Address,Description,Date,RequestStatusId,RequestTypeId")] Request requester, IFormFile[] files)
  {
    if (id != requester.Id)
    {
      return NotFound();
    }
    var errors = ValidateRequester(requester);

    // Eğer hata varsa, hata mesajlarını ViewBag'e ekle
    if (errors.Any())
    {
      ViewBag.Errors = errors;
      ViewBag.RequestStatusList = new SelectList(_context.RequestStatuses, "Id", "Status");
      ViewBag.RequestTypeList = new SelectList(_context.RequestTypes, "Id", "Type");
      return View(requester); // Formu tekrar yükle ve hataları göster
    }
    try
    {
      var existingRequester = await _context.Requests.FindAsync(id);
      if (existingRequester == null)
      {
        return NotFound();
      }

      existingRequester.Tckn = requester.Tckn;
      existingRequester.FirstName = requester.FirstName;
      existingRequester.Surname = requester.Surname;
      existingRequester.TelNo = requester.TelNo;
      existingRequester.Email = requester.Email;
      existingRequester.Address = requester.Address;
      existingRequester.Description = requester.Description;
      existingRequester.Date = DateTime.Now;
      existingRequester.RequestStatusId = requester.RequestStatusId;
      existingRequester.RequestTypeId = requester.RequestTypeId;

      // Yeni dosya yükleme
      if (files != null && files.Length > 0)
      {
        var filePaths = new List<FilePath>();

        foreach (var file in files)
        {
          if (file.Length > 0)
          {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
              await file.CopyToAsync(stream);
            }

            filePaths.Add(new FilePath { Filepath = "/uploads/" + file.FileName });
          }
        }

        // Yeni dosyaları RequestFilePath tablosuna ekleme
        foreach (var filePath in filePaths)
        {
          var requestFilePath = new RequestFilePath
          {
            FilePath = filePath,
            Requester = existingRequester
          };
          _context.Add(requestFilePath);
        }
      }

      _context.Update(existingRequester);
      await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
      if (!_context.Requests.Any(e => e.Id == requester.Id))
      {
        return NotFound();
      }
      else
      {
        throw;
      }
    }

    return RedirectToAction(nameof(Index));
  }
  // GET: Requester/Delete/{id}
  public async Task<IActionResult> Delete(int id)
  {
    var requester = await _context.Requests.FindAsync(id);
    if (requester == null)
    {
      return NotFound();
    }

    return View(requester);
  }

  [HttpPost]
  public async Task<IActionResult> DeleteConfirmed(int id)
  {
    var requester = await _context.Requests.FindAsync(id);
    if (requester == null)
    {
      return Json(new { success = false, message = "Kayıt bulunamadı!" });
    }

    try
    {
      // Bağlı dosyaları silme
      //var files = _context.RequestFilePaths.Where(f => f.Requester.Id == id).ToList();
      //foreach (var file in files)
      //{
      //  var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePath.Filepath);
      //  if (System.IO.File.Exists(filePath))
      //  {
      //    System.IO.File.Delete(filePath);
      //  }
      //  _context.RequestFilePaths.Remove(file);
      //}

      _context.Requests.Remove(requester);
      await _context.SaveChangesAsync();

      return Json(new { success = true });
    }
    catch (Exception ex)
    {
      return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
    }
  }

  public JsonResult GetDetails(int id)
  {
    var requester = _context. Requests.Include(r => r.RequestType).Include(r => r.RequestStatus)
                                       .FirstOrDefault(r => r.Id == id);

    if (requester == null)
    {
      return Json(new { success = false, message = "Talep bulunamadı" });
    }

    var result = new
    {
      tckn = requester.Tckn,
      firstName = requester.FirstName,
      surname = requester.Surname,
      requestType = requester.RequestType.Type,
      requestStatus = requester.RequestStatus.Status,
      date = requester.Date.ToString("yyyy-MM-dd"),
      description = requester.Description
    };
    return Json(result); // Veriyi JSON formatında döndürüyoruz
  }


  private List<string> ValidateRequester(Request model)
  {
    var errors = new List<string>();

    if (string.IsNullOrEmpty(model.Tckn) || model.Tckn.Length != 11)
    {
      errors.Add("T.C Kimlik No 11 haneli olmalıdır.");
    }

    if (string.IsNullOrEmpty(model.FirstName))
    {
      errors.Add("İsim zorunludur.");
    }

    if (string.IsNullOrEmpty(model.Surname))
    {
      errors.Add("Soyisim zorunludur.");
    }

    if (string.IsNullOrEmpty(model.Email))
    {
      errors.Add("Geçersiz E-Mail adresi.");
    }

    if (string.IsNullOrEmpty(model.TelNo) || model.TelNo.Length != 11)
    {
      errors.Add("Telefon numarası 11 haneli olmalıdır.");
    }

    if (string.IsNullOrEmpty(model.Address))
    {
      errors.Add("Adres zorunludur.");
    }
    if (string.IsNullOrEmpty(model.Description))
    {
      errors.Add("Açıklama zorunludur.");
    }

    if (model.RequestStatusId <= 0)
    {
      errors.Add("Talep durumu seçilmelidir.");
    }

    if (model.RequestTypeId <= 0)
    {
      errors.Add("Talep tipi seçilmelidir.");
    }

    return errors;
  }

}



