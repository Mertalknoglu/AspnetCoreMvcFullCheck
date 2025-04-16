using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspnetCoreMvcFull.Controllers
{
  public class AccountController : Controller
  {
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
      _signInManager = signInManager;
      _userManager = userManager;
    }
    #region Login
    [HttpGet]
    public IActionResult Login()
    {
      return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      // TC Kimlik Numarası ile kullanıcıyı bul
      var user = await _userManager.Users.FirstOrDefaultAsync(u => u.TcKimlikNo == model.TcKimlikNo);
      if (user == null)
      {
        ModelState.AddModelError("", "Kullanıcı bulunamadı.");
        return View(model);
      }

      // Kullanıcıyı giriş yap
      var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
      if (result.Succeeded)
      {
        TempData["UserName"] = user.FirstName;
        TempData["SurName"] = user.LastName;

        return RedirectToAction("Index", "Request");
      }

      ModelState.AddModelError("", "Giriş başarısız. TC Kimlik Numarası veya şifre hatalı.");
      return View(model);
    }

    #endregion
    #region Register
    [HttpGet]
    public IActionResult Register()
    {
      return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
      if (ModelState.IsValid)
      {
        // Yeni kullanıcıyı oluştur
        var user = new ApplicationUser
        {
          UserName = model.Email,  // Kullanıcı adı olarak e-posta
          Email = model.Email,
          FirstName = model.FirstName,
          LastName = model.LastName,
          TcKimlikNo = model.TcKimlikNo,
          PhoneNumber = model.PhoneNumber // Telefon numarasını ekle
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
          // Yeni kullanıcı oluşturulduktan sonra giriş işlemi yapılabilir
          await _signInManager.SignInAsync(user, isPersistent: false);
          return RedirectToAction("Index", "Request");
        }

        foreach (var error in result.Errors)
        {
          ModelState.AddModelError(string.Empty, error.Description);
        }
      }

      return View(model);
    }
    #endregion
    public async Task<IActionResult> Index()
    {
      // Kullanıcı oturum açmamışsa Login sayfasına yönlendir
      if (!User.Identity.IsAuthenticated)
      {
        return RedirectToAction("Login", "Account");
      }

      var user = await _userManager.GetUserAsync(User);
      if (user == null)
      {
        return RedirectToAction("Login", "Account");
      }

      ViewBag.FirstName = user.FirstName;
      ViewBag.LastName = user.LastName;
      return View();
    }
    #region Logout
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
      // Kullanıcıyı çıkış yap
      await _signInManager.SignOutAsync();

      // Çıkış işlemi sonrası Login sayfasına yönlendir
      return RedirectToAction("Login", "Account");
    }
    #endregion
    public async Task<IActionResult> Profile()
    {
      var user = await _userManager.GetUserAsync(User);
      if (user == null)
      {
        return RedirectToAction("Login", "Account");
      }
      ViewBag.FirstName = user.FirstName;
      ViewBag.LastName = user.LastName;
      return View(user);
    }
  }
}
