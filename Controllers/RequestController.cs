using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using AspnetCoreMvcFull.Models.Models;
using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Identity;
[Authorize]
public class RequestController : Controller
{
  private readonly ApplicationDbContext _context;
  private readonly UserManager<ApplicationUser> _userManager;

  public RequestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
  {
    _context = context;

    _userManager = userManager;
  }

  public async Task<IActionResult> Index()
  {
    var user = await _userManager.GetUserAsync(User);
    ViewBag.IsAdmin = user.IsAdmin;

    List<Request> requesters;

    if (user.IsAdmin)
    {
      ViewBag.GelenTalepler = await _context.Requests
          .Where(r => !r.IsDeleted)
          .Include(r => r.RequestStatus)
          .Include(r => r.RequestType)
          .Include(r => r.RequestUnit)
          .ToListAsync();
    }
    else
    {
      // Kullanıcıysa iki ayrı liste gönder
      ViewBag.GelenTalepler = await _context.Requests
          .Where(r => !r.IsDeleted && r.RequestUnitId == user.UnitId)
          .Include(r => r.RequestStatus)
          .Include(r => r.RequestType)
          .Include(r => r.RequestUnit)
          .ToListAsync();


      requesters = new List<Request>(); // Boş liste döndür
    }

    ViewBag.GonderdigimTalepler = await _context.Requests
        .Where(r => !r.IsDeleted && r.UserId == user.Id)
        .Include(r => r.RequestStatus)
        .Include(r => r.RequestType)
        .Include(r => r.RequestUnit)
        .ToListAsync();

    ViewBag.RequestStatusList = _context.RequestStatuses
        .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Status }).ToList();

    ViewBag.RequestTypeList = _context.RequestTypes
        .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Type }).ToList();

    ViewBag.RequestUnitList = _context.RequestUnits
        .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Unit }).ToList();

    return View(); // admin için çalışır, kullanıcıda boş döner
  }



  // GET: Requester/Create
  public IActionResult Create()
  {
    // RequestStatuses ve RequestTypes listesini ViewData yerine ViewBag üzerinden gönderiyoruz.
    ViewBag.RequestStatusList = new SelectList(_context.RequestStatuses, "Id", "Status");
    ViewBag.RequestTypeList = new SelectList(_context.RequestTypes, "Id", "Type");
    ViewBag.RequestUnitList = new SelectList(_context.RequestUnits, "Id", "Unit");

    return View();
  }


  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create([Bind("Tckn,FirstName,Surname,TelNo,Email,Address,Description,RequestUnitId,RequestStatusId,RequestTypeId")] Request requester, IFormFile[] files)
  {
    // Giriş yapan kullanıcının Id'sini alıyoruz
    var user = await _userManager.GetUserAsync(User);
    var userID = user.Id;

    var errors = ValidateRequester(requester);

    if (errors.Any())
    {
      ViewBag.Errors = errors;
      ViewBag.RequestStatusList = new SelectList(_context.RequestStatuses, "Id", "Status");
      ViewBag.RequestTypeList = new SelectList(_context.RequestTypes, "Id", "Type");
      ViewBag.RequestUnitList = new SelectList(_context.RequestUnits, "Id", "Unit");
      return View(requester);
    }

    requester.Date = DateTime.Now;
    requester.UserId = userID; // 🔥 Giriş yapan kullanıcıya set et
    requester.CreatedAt = DateTime.Now;
    requester.CreatedBy = user.Id;
    requester.ModifiedAt = DateTime.Now;
    requester.IsDeleted = false;
    // Dosya yükleme kısmı
    if (files != null && files.Length > 0)
    {
      var filePaths = new List<RequestFilePath>();

      foreach (var file in files)
      {
        if (file.Length > 0)
        {
          var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", file.FileName);

          using (var stream = new FileStream(filePath, FileMode.Create))
          {
            await file.CopyToAsync(stream);
          }

          filePaths.Add(new RequestFilePath { FilePaths = "/uploads/" + file.FileName });
        }
      }

      foreach (var filePath in filePaths)
      {
        filePath.Requester = requester;
        _context.Add(filePath);
      }

    }

    _context.Add(requester);
    await _context.SaveChangesAsync();
    TempData["SuccessMessage"] = "Kayıt başarıyla oluşturuldu!";
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
    ViewBag.RequestUnitList = new SelectList(_context.RequestUnits, "Id", "Unit", requester.RequestUnitId);


    return View(requester);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(int id, [Bind("Id,Tckn,FirstName,Surname,TelNo,Email,Address,Description,RequestStatusId,RequestTypeId,RequestUnitId")] Request requester, [FromForm] IFormFile[] newFiles)
  {
    if (id != requester.Id)
      return NotFound();

    var existingRequester = await _context.Requests.FindAsync(id);
    if (existingRequester == null)
      return NotFound();

    var user = await _userManager.GetUserAsync(User);
    var userId = user.Id;
    var fullName = $"{user.FirstName} {user.LastName}";

    // LOG: Durum
    var oldStatus = await _context.RequestStatuses.FindAsync(existingRequester.RequestStatusId);
    var newStatus = await _context.RequestStatuses.FindAsync(requester.RequestStatusId);
    await LogIfChanged(existingRequester.Id, "Durum Güncellendi", oldStatus?.Status, newStatus?.Status, fullName);
    existingRequester.RequestStatusId = requester.RequestStatusId;

    // LOG: Tip
    var oldType = await _context.RequestTypes.FindAsync(existingRequester.RequestTypeId);
    var newType = await _context.RequestTypes.FindAsync(requester.RequestTypeId);
    await LogIfChanged(existingRequester.Id, "Tip Güncellendi", oldType?.Type, newType?.Type, fullName);
    existingRequester.RequestTypeId = requester.RequestTypeId;

    // LOG: Birim
    var oldUnit = await _context.RequestUnits.FindAsync(existingRequester.RequestUnitId);
    var newUnit = await _context.RequestUnits.FindAsync(requester.RequestUnitId);
    await LogIfChanged(existingRequester.Id, "Birim Güncellendi", oldUnit?.Unit, newUnit?.Unit, fullName);
    existingRequester.RequestUnitId = requester.RequestUnitId;
    // Güncellenebilir alanlar
    existingRequester.Tckn = requester.Tckn;
    existingRequester.FirstName = requester.FirstName;
    existingRequester.Surname = requester.Surname;
    existingRequester.TelNo = requester.TelNo;
    existingRequester.Email = requester.Email;
    existingRequester.Address = requester.Address;
    existingRequester.Description = requester.Description;
    existingRequester.RequestStatusId = requester.RequestStatusId;
    existingRequester.RequestTypeId = requester.RequestTypeId;
    existingRequester.RequestUnitId = requester.RequestUnitId;

    // Audit alanları
    existingRequester.ModifiedAt = DateTime.Now;
    existingRequester.ModifiedBy = userId;

    if (newFiles != null && newFiles.Length > 0)
    {
      foreach (var file in newFiles)
      {
        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", uniqueFileName);

        using (var stream = new FileStream(path, FileMode.Create))
        {
          await file.CopyToAsync(stream);
        }

        _context.RequestFilePaths.Add(new RequestFilePath
        {
          FilePaths = "/uploads/" + uniqueFileName,
          RequestDetailsId = existingRequester.Id  // Dikkat! existingRequester.Id olmalı
        });
      }
    }

    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Talep başarıyla güncellendi.";
    return RedirectToAction(nameof(Index));
  }

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
      return Json(new { success = false, message = "Talep bulunamadı!" });
    }

    var user = await _userManager.GetUserAsync(User);

    // ❌ Eğer talebi oluşturan kullanıcıysa silmeye izin verme
    if (requester.UserId == user.Id)
    {
      return Json(new { success = false, message = "Kendi oluşturduğunuz talebi silemezsiniz." });
    }

    requester.IsDeleted = true;
    requester.ModifiedAt = DateTime.Now;
    requester.ModifiedBy = user.Id;

    _context.Update(requester);
    await _context.SaveChangesAsync();

    return Json(new { success = true });
  }


  [HttpPost]
  public async Task<IActionResult> EditAjax(int id, [FromForm] Request requester, [FromForm] IFormFile[] NewFiles)
  {
    var existing = await _context.Requests.FindAsync(id);
    if (existing == null)
      return Json(new { success = false, message = "Talep bulunamadı." });

    var user = await _userManager.GetUserAsync(User);
    var fullName = $"{user.FirstName} {user.LastName}";

    // Durum logla ve güncelle
    var oldStatus = await _context.RequestStatuses.FindAsync(existing.RequestStatusId);
    var newStatus = await _context.RequestStatuses.FindAsync(requester.RequestStatusId);
    await LogIfChanged(existing.Id, "Durum Güncellendi", oldStatus?.Status, newStatus?.Status, fullName);
    existing.RequestStatusId = requester.RequestStatusId;

    // Tip logla ve güncelle
    var oldType = await _context.RequestTypes.FindAsync(existing.RequestTypeId);
    var newType = await _context.RequestTypes.FindAsync(requester.RequestTypeId);
    await LogIfChanged(existing.Id, "Tip Güncellendi", oldType?.Type, newType?.Type, fullName);
    existing.RequestTypeId = requester.RequestTypeId;

    // Birim logla ve güncelle
    var oldUnit = await _context.RequestUnits.FindAsync(existing.RequestUnitId);
    var newUnit = await _context.RequestUnits.FindAsync(requester.RequestUnitId);
    await LogIfChanged(existing.Id, "Birim Güncellendi", oldUnit?.Unit, newUnit?.Unit, fullName);
    existing.RequestUnitId = requester.RequestUnitId;
    existing.ModifiedAt = DateTime.Now;
    existing.ModifiedBy = user.Id;

    if (NewFiles != null && NewFiles.Length > 0)
    {
      foreach (var file in NewFiles)
      {
        var uniqueFileName = Guid.NewGuid() + "_" + file.FileName;
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", uniqueFileName);

        using (var stream = new FileStream(path, FileMode.Create))
        {
          await file.CopyToAsync(stream);
        }

        _context.RequestFilePaths.Add(new RequestFilePath
        {
          RequestDetailsId = existing.Id,
          FilePaths = "/uploads/" + uniqueFileName
        });
      }
    }

    await _context.SaveChangesAsync();

    return Json(new { success = true });
  }



  public JsonResult GetDetails(int id)
  {
    var logs = _context.RequestLogs
     .Where(l => l.RequestId == id)
     .OrderByDescending(l => l.ChangedAt)
     .Select(l => new
     {
       l.ActionType,
       l.Description,
       l.ChangedBy,
       Date = l.ChangedAt.ToString("dd.MM.yyyy HH:mm")
     }).ToList();

    var requester = _context.Requests
        .Include(r => r.RequestType)
        .Include(r => r.RequestStatus)
        .Include(r => r.RequestFilePaths)
        .FirstOrDefault(r => r.Id == id);

    if (requester == null)
      return Json(new { success = false, message = "Talep bulunamadı" });

    var result = new
    {
      success = true,
      tckn = requester.Tckn,
      firstName = requester.FirstName,
      surname = requester.Surname,
      requestType = requester.RequestType?.Type,
      requestStatus = requester.RequestStatus?.Status,
      date = requester.Date.ToString("yyyy-MM-dd"),
      description = requester.Description,
      files = requester.RequestFilePaths.Select(f => f.FilePaths).ToList(),
      logs = logs
    };

    return Json(result);

  }





  [HttpPost]
  public async Task<IActionResult> UpdateStatusUnitType(int Id, int RequestStatusId, int RequestTypeId, int RequestUnitId)
  {
    var requester = await _context.Requests.FindAsync(Id);
    if (requester == null)
    {
      return Json(new { success = false, message = "Talep bulunamadı." });
    }
    var user = await _userManager.GetUserAsync(User);
    var fullName = $"{user.FirstName} {user.LastName}";

    // Durum
    var oldStatus = await _context.RequestStatuses.FindAsync(requester.RequestStatusId);
    var newStatus = await _context.RequestStatuses.FindAsync(RequestStatusId);
    await LogIfChanged(requester.Id, "Durum Güncellendi", oldStatus?.Status, newStatus?.Status, fullName);
    requester.RequestStatusId = RequestStatusId;

    // Tip
    var oldType = await _context.RequestTypes.FindAsync(requester.RequestTypeId);
    var newType = await _context.RequestTypes.FindAsync(RequestTypeId);
    await LogIfChanged(requester.Id, "Tip Güncellendi", oldType?.Type, newType?.Type, fullName);
    requester.RequestTypeId = RequestTypeId;

    // Birim
    var oldUnit = await _context.RequestUnits.FindAsync(requester.RequestUnitId);
    var newUnit = await _context.RequestUnits.FindAsync(RequestUnitId);
    await LogIfChanged(requester.Id, "Birim Güncellendi", oldUnit?.Unit, newUnit?.Unit, fullName);
    requester.RequestUnitId = RequestUnitId;
    requester.ModifiedAt = DateTime.Now;
    requester.ModifiedBy = user.Id;

    await _context.SaveChangesAsync();

    return Json(new { success = true });
  }
  [HttpGet]
  public IActionResult GetFiles(int id)
  {
    var files = _context.RequestFilePaths
        .Where(x => x.RequestDetailsId == id)
        .Select(x => x.FilePaths)
        .ToList();

    return Json(files);
  }


  [HttpPost]
  public IActionResult DeleteFile([FromBody] FileDeleteDto dto)
  {
    var file = _context.RequestFilePaths
        .FirstOrDefault(x => x.FilePaths == dto.FilePath && x.RequestDetailsId == dto.RequestId);

    if (file != null)
    {
      _context.RequestFilePaths.Remove(file);
      _context.SaveChanges();

      var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", file.FilePaths.TrimStart('/'));
      if (System.IO.File.Exists(fullPath))
        System.IO.File.Delete(fullPath);

      return Ok();
    }

    return BadRequest("Dosya bulunamadı");
  }
  // Mevcut Index metodunu bozmadan filtreleme için alternatif action:
  [HttpGet]
  public async Task<IActionResult> FilteredIndex(string name, string type, string unit, string status)
  {
    var user = await _userManager.GetUserAsync(User);
    ViewBag.IsAdmin = user.IsAdmin;

    IQueryable<Request> query = _context.Requests
        .Where(r => !r.IsDeleted)
        .Include(r => r.RequestStatus)
        .Include(r => r.RequestType)
        .Include(r => r.RequestUnit);

    // Kullanıcı Admin değilse, birime göre filtrele
    if (!user.IsAdmin)
    {
      query = query.Where(r => r.RequestUnitId == user.UnitId || r.UserId == user.Id);
    }

    // Filtreleme kriterleri:
    if (!string.IsNullOrWhiteSpace(name))
      query = query.Where(r => (r.FirstName + " " + r.Surname).Contains(name));

    if (!string.IsNullOrWhiteSpace(type))
      query = query.Where(r => r.RequestType.Type.Contains(type));

    if (!string.IsNullOrWhiteSpace(unit))
      query = query.Where(r => r.RequestUnit.Unit.Contains(unit));

    if (!string.IsNullOrWhiteSpace(status))
      query = query.Where(r => r.RequestStatus.Status.Contains(status));

    var filteredRequests = await query.ToListAsync();

    // ViewBag verileri mevcut yapı ile uyumlu
    ViewBag.GelenTalepler = filteredRequests.Where(r => r.RequestUnitId == user.UnitId || user.IsAdmin).ToList();
    ViewBag.GonderdigimTalepler = filteredRequests.Where(r => r.UserId == user.Id).ToList();

    ViewBag.RequestStatusList = _context.RequestStatuses.Select(x => new SelectListItem
    {
      Value = x.Id.ToString(),
      Text = x.Status
    }).ToList();

    ViewBag.RequestTypeList = _context.RequestTypes.Select(x => new SelectListItem
    {
      Value = x.Id.ToString(),
      Text = x.Type
    }).ToList();

    ViewBag.RequestUnitList = _context.RequestUnits.Select(x => new SelectListItem
    {
      Value = x.Id.ToString(),
      Text = x.Unit
    }).ToList();

    return View("Index"); // Aynı Index.cshtml sayfasına yönlendir
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
  private async Task LogIfChanged<T>(
    int requestId,
    string actionType,
    T oldValue,
    T newValue,
    string changedBy,
    string? displayOld = null,
    string? displayNew = null)
  {
    if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
    {
      string oldVal = displayOld ?? oldValue?.ToString() ?? "";
      string newVal = displayNew ?? newValue?.ToString() ?? "";

      _context.RequestLogs.Add(new RequestLog
      {
        RequestId = requestId,
        ActionType = actionType,
        Description = $"{oldVal} → {newVal}",
        ChangedBy = changedBy,
        ChangedAt = DateTime.Now
      });
    }
  }

}
