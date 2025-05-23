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

    ViewBag.TamamlananTalepler = await _context.Requests
    .Where(r => !r.IsDeleted && r.UserId == user.Id && r.RequestStatus.Status == "Tamamlandı")
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
    var user = _userManager.GetUserAsync(User);
    bool isAdmin = user.Result.IsAdmin;

    // "Genel Sekreter" biriminin Id'sini aldık
    var allUnits = _context.RequestUnits.ToListAsync();
    var genelSekreter = allUnits.Result.FirstOrDefault(u => u.Unit == "Sekreterlik Başkanı Yürütme Kurulu Üyesi");
    int gsUnitId = genelSekreter?.Id ?? 0;

    // Eğer Admin değilse VE kullanıcının birimi GS değilse => disable et
    bool disableUnitSelect = !isAdmin && user.Result.UnitId != gsUnitId;

    ViewBag.DisableUnitSelect = disableUnitSelect;
    ViewBag.GeneralSekreterUnitId = gsUnitId;

    // Tüm birimleri liste olarak gönderiyoruz
    ViewBag.RequestUnitList = new SelectList(
        items: allUnits.Result,
        dataValueField: "Id",
        dataTextField: "Unit",
        selectedValue: gsUnitId     // default GS seçili
    );

    var statuses = _context.RequestStatuses.ToList();
    var defaultStatusId = statuses.FirstOrDefault(s => s.Status == "İşlemde")?.Id;
    ViewBag.RequestStatusList = new SelectList(statuses, "Id", "Status", defaultStatusId);
    ViewBag.RequestTypeList = new SelectList(_context.RequestTypes, "Id", "Type");

    return View();
  }


  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create([Bind("Tckn,FirstName,Surname,TelNo,Email,Address,Description,RequestUnitId,RequestStatusId,RequestTypeId,Response")] Request requester, IFormFile[] files)
  {
    // Giriş yapan kullanıcının Id'sini alıyoruz
    var user = await _userManager.GetUserAsync(User);
    var userID = user.Id;

    var fullName = $"{user.FirstName} {user.LastName}";
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

    var actor = $"{user.FirstName} {user.LastName}";
    var statusName = _context.RequestStatuses.FirstOrDefault(s => s.Id == requester.RequestStatusId)?.Status;
    var typeName = _context.RequestTypes.FirstOrDefault(t => t.Id == requester.RequestTypeId)?.Type;
    var unitName = _context.RequestUnits.FirstOrDefault(u => u.Id == requester.RequestUnitId)?.Unit;

    await LogIfChanged<string>(
        requester.Id,
        actionType: "Talep Oluşturuldu",
        oldValue: string.Empty,
        newValue: $"Durum: {statusName}, Tip: {typeName}, Birim: {unitName}",  // herhangi bir dummy değer, sadece eşit olmaması yeterli
        changedBy: fullName,
        response: requester.Response
    );
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
  public async Task<IActionResult> Edit(int id, [Bind("Id,Tckn,FirstName,Surname,TelNo,Email,Address,Description,RequestStatusId,RequestTypeId,RequestUnitId,Response")] Request requester, [FromForm] IFormFile[] newFiles)
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
    existingRequester.Response = requester.Response;
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

    requester.IsDeleted = true;
    requester.ModifiedAt = DateTime.Now;
    requester.ModifiedBy = user.Id;

    _context.Update(requester);
    await _context.SaveChangesAsync();

    return Json(new { success = true });
  }


  [HttpPost]
  public async Task<IActionResult> EditAjax(int id, [FromForm] Request requester, [FromForm] IFormFile[] NewFiles, [FromForm] string? response)
  {
    var existing = await _context.Requests.FindAsync(id);
    if (existing == null)
      return Json(new { success = false, message = "Talep bulunamadı." });

    var user = await _userManager.GetUserAsync(User);
    var fullName = $"{user.FirstName} {user.LastName}";

    // Durum logla ve güncelle
    var oldStatus = await _context.RequestStatuses.FindAsync(existing.RequestStatusId);
    var newStatus = await _context.RequestStatuses.FindAsync(requester.RequestStatusId);
    await LogIfChanged(existing.Id, "Durum Güncellendi", oldStatus?.Status, newStatus?.Status, fullName, response: response);
    existing.RequestStatusId = requester.RequestStatusId;

    // Tip logla ve güncelle
    var oldType = await _context.RequestTypes.FindAsync(existing.RequestTypeId);
    var newType = await _context.RequestTypes.FindAsync(requester.RequestTypeId);
    await LogIfChanged(existing.Id, "Tip Güncellendi", oldType?.Type, newType?.Type, fullName, response: response);
    existing.RequestTypeId = requester.RequestTypeId;

    // Birim logla ve güncelle
    var oldUnit = await _context.RequestUnits.FindAsync(existing.RequestUnitId);
    var newUnit = await _context.RequestUnits.FindAsync(requester.RequestUnitId);
    await LogIfChanged(existing.Id, "Birim Güncellendi", oldUnit?.Unit, newUnit?.Unit, fullName, response: response);

    var oldResp = existing.Response;
    if (!string.IsNullOrWhiteSpace(response) && response != oldResp)
    {
      await LogIfChanged(
          existing.Id,
          "Yanıt Güncellendi",
          oldResp,           // eski
          response,          // yeni
          fullName,
          response: response // response’ı da buraya düşür
      );
      existing.Response = response;
    }
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
    var requester = _context.Requests
        .Include(r => r.RequestType)
        .Include(r => r.RequestStatus)
        .Include(r => r.RequestUnit)
        .Include(r => r.RequestFilePaths)
        .FirstOrDefault(r => r.Id == id);

    if (requester == null)
      return Json(new { success = false, message = "Talep bulunamadı" });

    var logs = _context.RequestLogs
        .Where(l => l.RequestId == id)
        .OrderByDescending(l => l.ChangedAt)
        .Select(l => new
        {
          l.ActionType,
          l.Description,
          l.ChangedBy,
          Date = l.ChangedAt.ToString("dd.MM.yyyy HH:mm"),
          l.Response
        }).ToList();

    return Json(new
    {
      success = true,
      tckn = requester.Tckn,
      firstName = requester.FirstName,
      surname = requester.Surname,
      date = requester.Date.ToString("yyyy-MM-dd"),
      description = requester.Description,

      // --- BURAYA EKLENDİ ---
      requestTypeId = requester.RequestTypeId,
      requestStatusId = requester.RequestStatusId,
      requestUnitId = requester.RequestUnitId,

      requestType = requester.RequestType?.Type,
      requestStatus = requester.RequestStatus?.Status,
      requestUnit = requester.RequestUnit?.Unit,

      files = requester.RequestFilePaths.Select(f => f.FilePaths).ToList(),
      logs = logs
    });
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
  public async Task<IActionResult> FilteredIndex(string? startDate, string? endDate, int? statusId, int? typeId, int? unitId)
  {
    var user = await _userManager.GetUserAsync(User);
    ViewBag.IsAdmin = user.IsAdmin;

    IQueryable<Request> query = _context.Requests
        .Include(x => x.RequestType)
        .Include(x => x.RequestStatus)
        .Include(x => x.RequestUnit)
        .Where(r => !r.IsDeleted);

    if (!user.IsAdmin)
    {
      query = query.Where(r => r.RequestUnitId == user.UnitId || r.UserId == user.Id);
    }

    if (statusId.HasValue)
    {
      query = query.Where(r => r.RequestStatusId == statusId);
      ViewBag.SelectedStatusId = statusId.ToString();
    }

    if (typeId.HasValue)
    {
      query = query.Where(r => r.RequestTypeId == typeId);
      ViewBag.SelectedTypeId = typeId.ToString();
    }

    if (unitId.HasValue)
    {
      query = query.Where(r => r.RequestUnitId == unitId);
      ViewBag.SelectedUnitId = unitId.ToString();
    }

    if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
    {
      query = query.Where(r => r.Date >= start);
      ViewBag.StartDate = start.ToString("yyyy-MM-dd");
    }

    if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
    {
      // Ertesi günün 00:00:00'ına kadar alır, böylece aynı gün dahil olur
      var inclusiveEnd = end.AddDays(1);
      query = query.Where(r => r.Date < inclusiveEnd);
      ViewBag.EndDate = end.ToString("yyyy-MM-dd");
    }


    var filtered = await query.ToListAsync();

    ViewBag.GelenTalepler = filtered.Where(r => r.RequestUnitId == user.UnitId || user.IsAdmin).ToList();
    ViewBag.GonderdigimTalepler = filtered.Where(r => r.UserId == user.Id).ToList();
    ViewBag.TamamlananTalepler = filtered.Where(r => r.UserId == user.Id && r.RequestStatus.Status == "Tamamlandı")
    .ToList();

    ViewBag.RequestStatusList = _context.RequestStatuses.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Status }).ToList();
    ViewBag.RequestTypeList = _context.RequestTypes.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Type }).ToList();
    ViewBag.RequestUnitList = _context.RequestUnits.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Unit }).ToList();

    return View("Index");
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
    string? displayNew = null,
    string? response = null)
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
        ChangedAt = DateTime.Now,
        Response = response
      });
    }
  }
  public async Task<IActionResult> History(string tckn)
  {
    // tckn ile daha önce kaydedilmiş tüm talepleri çekin
    var list = await _context.Requests
                      .Where(r => r.Tckn == tckn && !r.IsDeleted)
                      .Include(r => r.RequestStatus)
                      .Include(r => r.RequestType)
                      .Include(r => r.RequestUnit)
                      .OrderByDescending(r => r.Date)
                      .ToListAsync();

    ViewData["Tckn"] = tckn;
    return View(list);  // Views/Request/History.cshtml
  }
  [HttpGet]
  public JsonResult CheckHistory(string tckn)
  {
    var count = _context.Requests
        .Count(r => r.Tckn == tckn && !r.IsDeleted);

    return Json(new
    {
      hasHistory = (count > 0),
      count = count
    });
  }
}
