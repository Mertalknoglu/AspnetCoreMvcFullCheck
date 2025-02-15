using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

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
    var requesters = await _context.Requesters
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
  public async Task<IActionResult> Create([Bind("Tckn,FirstName,Surname,TelNo,Email,Address,Description,Date,RequestStatusId,RequestTypeId")] Requester requester, IFormFile[] files)
  {
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

    return RedirectToAction(nameof(Index));
  }




  // POST: Requester/Edit/5
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(int id, [Bind("Id,Tckn,FirstName,Surname,TelNo,Email,Address,Description,Date,RequestStatusId,RequestTypeId")] Requester requester)
  {
    if (id != requester.Id)
    {
      return NotFound();
    }

    try
    {
      _context.Update(requester);
      await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
      if (!RequesterExists(requester.Id))
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

  // GET: Requester/Delete/5
  public async Task<IActionResult> Delete(int? id)
  {
    if (id == null)
    {
      return NotFound();
    }

    var requester = await _context.Requesters
        .Include(r => r.RequestStatus)
        .Include(r => r.RequestType)
        .FirstOrDefaultAsync(m => m.Id == id);
    if (requester == null)
    {
      return NotFound();
    }

    return View(requester);
  }

  // POST: Requester/Delete/5
  [HttpPost, ActionName("Delete")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> DeleteConfirmed(int id)
  {
    var requester = await _context.Requesters.FindAsync(id);
    _context.Requesters.Remove(requester);
    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
  }

  private bool RequesterExists(int id)
  {
    return _context.Requesters.Any(e => e.Id == id);
  }
}
