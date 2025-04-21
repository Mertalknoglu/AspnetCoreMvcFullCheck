using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using AspnetCoreMvcFull.Models.ViewModel;
using AspnetCoreMvcFull.Filters;

namespace AspnetCoreMvcFull.Controllers
{
  [AdminOnly]
  public class UserController : Controller
  {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
      _context = context;
      _userManager = userManager;
    }


    public async Task<IActionResult> Index()
    {
      var users = await _userManager.Users
                    .Include(u => u.Unit)
                    .ToListAsync();

      ViewBag.Units = _context.RequestUnits
                       .Select(u => new SelectListItem
                       {
                         Value = u.Id.ToString(),
                         Text = u.Unit
                       }).ToList();

      return View(users);
    }


    [HttpGet]
    public async Task<IActionResult> GetById(int id)
    {
      var user = await _userManager.Users.Include(x => x.Unit).FirstOrDefaultAsync(x => x.Id == id);
      if (user == null)
        return Json(new { success = false, message = "Kullanıcı bulunamadı." });

      return Json(new
      {
        success = true,
        data = new
        {
          user.Id,
          user.FirstName,
          user.LastName,
          user.Email,
          user.PhoneNumber,
          user.TcKimlikNo,
          user.UnitId,
          user.IsAdmin
        }
      });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ApplicationUser model)
    {
      var user = await _userManager.FindByIdAsync(model.Id.ToString());
      if (user == null) return Json(new { success = false, message = "Kullanıcı bulunamadı." });

      user.FirstName = model.FirstName;
      user.LastName = model.LastName;
      user.Email = model.Email;
      user.UserName = model.Email;
      user.PhoneNumber = model.PhoneNumber;
      user.TcKimlikNo = model.TcKimlikNo;
      user.UnitId = model.UnitId;
      user.IsAdmin = model.IsAdmin;

      var result = await _userManager.UpdateAsync(user);
      return Json(new { success = result.Succeeded });
    }
    [HttpPost]
    public async Task<IActionResult> EditAjax(int id, [FromForm] UpdateUserViewModel model)
    {
      try
      {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
          return Json(new { success = false, message = "Kullanıcı bulunamadı." });

        // Şifre değişikliği
        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
          if (model.NewPassword != model.ConfirmPassword)
            return Json(new { success = false, message = "Şifreler uyuşmuyor." });

          var token = await _userManager.GeneratePasswordResetTokenAsync(user);
          var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
          if (!passwordResult.Succeeded)
          {
            var errorList = string.Join(", ", passwordResult.Errors.Select(e => e.Description));
            return Json(new { success = false, message = $"Şifre güncellenemedi: {errorList}" });
          }
        }

        // Diğer alanlar
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.TcKimlikNo = model.TcKimlikNo;
        user.UnitId = model.UnitId;
        user.IsAdmin = model.IsAdmin;

        var result = await _userManager.UpdateAsync(user);

        return Json(new
        {
          success = result.Succeeded,
          message = result.Succeeded ? "Kullanıcı başarıyla güncellendi." : "Güncelleme başarısız."
        });
      }
      catch (Exception ex)
      {
        return Json(new { success = false, message = $"Sunucu hatası: {ex.Message}" });
      }
    }





    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
      var user = await _userManager.FindByIdAsync(id.ToString());
      if (user == null) return Json(new { success = false });

      var result = await _userManager.DeleteAsync(user);
      return Json(new { success = result.Succeeded });
    }
    [HttpGet]
    [HttpGet]
    public IActionResult Create()
    {
      ViewBag.UnitList = new SelectList(_context.RequestUnits, "Id", "Unit");
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
      var errors = new List<string>();

      if (!ModelState.IsValid)
      {
        ViewBag.UnitList = new SelectList(_context.RequestUnits, "Id", "Unit");
        return View(model);
      }

      var user = new ApplicationUser
      {
        UserName = model.Email,
        Email = model.Email,
        FirstName = model.FirstName,
        LastName = model.LastName,
        PhoneNumber = model.PhoneNumber,
        TcKimlikNo = model.TcKimlikNo,
        UnitId = model.UnitId,
        IsAdmin = model.IsAdmin
      };

      var result = await _userManager.CreateAsync(user, model.Password);

      if (result.Succeeded)
      {
        TempData["SuccessMessage"] = "Kullanıcı başarıyla oluşturuldu.";
        return RedirectToAction("Index");
      }

      errors.AddRange(result.Errors.Select(e => e.Description));
      ViewBag.Errors = errors;
      ViewBag.UnitList = new SelectList(_context.RequestUnits, "Id", "Unit");
      return View(model);
    }




    private List<SelectListItem> GetUnitList()
    {
      return _context.RequestUnits
          .Select(u => new SelectListItem
          {
            Value = u.Id.ToString(),
            Text = u.Unit
          }).ToList();
    }
    [HttpGet]
    public IActionResult GetUser(int id)
    {
      var user = _context.Users.Include(x => x.Unit).FirstOrDefault(x => x.Id == id);
      if (user == null)
        return Json(new { success = false });

      return Json(new
      {
        success = true,
        id = user.Id,
        firstName = user.FirstName,
        lastName = user.LastName,
        email = user.Email,
        phoneNumber = user.PhoneNumber,
        tcKimlikNo = user.TcKimlikNo,
        unitId = user.UnitId,
        isAdmin = user.IsAdmin
      });
    }
  }
}
