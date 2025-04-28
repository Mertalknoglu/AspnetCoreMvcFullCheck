using AspnetCoreMvcFull.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using AspnetCoreMvcFull.Models.Models;

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
      var userUnit = await _userManager.Users
     .Include(u => u.Unit)
     .FirstOrDefaultAsync(u => u.TcKimlikNo == model.TcKimlikNo);



      // Kullanıcıyı giriş yap
      var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
      if (result.Succeeded)
      {
        TempData["UserName"] = user.FirstName;
        TempData["SurName"] = user.LastName;
        HttpContext.Session.SetString("UserFullName", user.FirstName + " " + user.LastName);
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetInt32("IsAdmin", user.IsAdmin ? 1 : 0);
        HttpContext.Session.SetString("UnitName", userUnit?.Unit?.Unit ?? "Tanımsız");
        HttpContext.Session.SetString("ProfilePicture", user.ProfilePicture ?? "1.png");

        return RedirectToAction("Dashboard", "Home"); ;
      }

      ModelState.AddModelError("", "Giriş başarısız. TC Kimlik Numarası veya şifre hatalı.");
      return View(model);
    }

    #endregion
    #region Register
    [HttpGet]
    public IActionResult Register()
    {
      if (!User.Identity.IsAuthenticated)
      {
        return RedirectToAction("Index", "Login");
      }
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
          PhoneNumber = model.PhoneNumber,
          ProfilePicture = "1.png"
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
          // Yeni kullanıcı oluşturulduktan sonra giriş işlemi yapılabilir
          await _signInManager.SignInAsync(user, isPersistent: false);
          return RedirectToAction("Dashboard", "Home");
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

      return View();
    }
    #region Logout
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
      // Kullanıcıyı çıkış yap
      await _signInManager.SignOutAsync();
      HttpContext.Session.Clear();
      // Çıkış işlemi sonrası Login sayfasına yönlendir
      return RedirectToAction("Login", "Account");
    }
    #endregion


    public IActionResult ForgotPassword()
    {
      return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
      if (!ModelState.IsValid)
        return View(model);

      var user = await _userManager.FindByEmailAsync(model.Email);
      if (user == null)
      {
        ModelState.AddModelError(nameof(model.Email), "Bu e-posta adresi sistemde bulunamadı.");
        return View(model);
      }

      // ✅ Güçlü şifre üret
      var newPassword = GenerateSecurePassword();

      // 🔁 Reset işlemi yap
      var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
      var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

      if (result.Succeeded)
      {
        // ✉️ Mail gönder
        await SendResetPasswordEmailAsync(model.Email, newPassword);

        // ✅ Popup göstermek için flag setle
        TempData["PopupSuccess"] = "Yeni şifreniz e-posta adresinize gönderildi. Giriş ekranına dönüp geçici şifreniz ile giriş yapaiblirsiniz.";
        return RedirectToAction("ForgotPassword");
      }


      foreach (var error in result.Errors)
        ModelState.AddModelError(string.Empty, error.Description);

      return View(model);
    }

    private string GenerateSecurePassword()
    {
      const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
      const string lower = "abcdefghijklmnopqrstuvwxyz";
      const string digits = "0123456789";
      const string symbols = "!@#$%^&*";

      var random = new Random();

      string password = string.Concat(
          upper[random.Next(upper.Length)],
          lower[random.Next(lower.Length)],
          digits[random.Next(digits.Length)],
          symbols[random.Next(symbols.Length)]
      );

      string allChars = upper + lower + digits + symbols;
      for (int i = 0; i < 4; i++)
      {
        password += allChars[random.Next(allChars.Length)];
      }

      return new string(password.OrderBy(c => random.Next()).ToArray());
    }

    private async Task SendResetPasswordEmailAsync(string email, string newPassword)
    {
      var fromAddress = new MailAddress("mertalknoglu12@gmail.com", "AK PARTİ'M"); // Gönderen
      var toAddress = new MailAddress(email);
      const string fromPassword = "oecj ljtu uwkg vexb"; // Gmail için özel uygulama şifresi gerekir
      const string subject = "Gecici Şifre";
      string body = $"Merhaba, <br><br> Talebiniz üzerine geçici şifreniz oluşturulmuştur:<br><br><strong>{newPassword}</strong><br><br>Lütfen giriş yaptıktan sonra şifrenizi değiştiriniz.";

      var smtp = new SmtpClient
      {
        Host = "smtp.gmail.com",
        Port = 587,
        EnableSsl = true,
        DeliveryMethod = SmtpDeliveryMethod.Network,
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
      };

      using (var message = new MailMessage(fromAddress, toAddress)
      {
        Subject = subject,
        Body = body,
        IsBodyHtml = true
      })
      {
        await smtp.SendMailAsync(message);
      }
    }
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
      var userId = int.Parse(_userManager.GetUserId(User));
      var user = await _userManager.Users
          .Include(u => u.Unit)
          .FirstOrDefaultAsync(u => u.Id == userId);


      if (user == null)
        return RedirectToAction("Login", "Account");

      var model = new ProfileViewModel
      {
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        UnitName = user.Unit?.Unit,
        IsAdmin = user.IsAdmin ? true : false,
        ProfilePicture = user.ProfilePicture
      };

      return View(model);
    }



    [HttpPost]
    public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
    {
      var user = await _userManager.GetUserAsync(User);

      if (user == null)
        return RedirectToAction("Login", "Account");

      // PROFİL RESMİ YÜKLEME
      if (model.UploadedPhoto != null && model.UploadedPhoto.Length > 0)
      {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var ext = Path.GetExtension(model.UploadedPhoto.FileName).ToLowerInvariant();

        if (allowedExtensions.Contains(ext) && model.UploadedPhoto.Length <= 800 * 1024)
        {
          var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "avatars");
          if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

          var fileName = string.IsNullOrWhiteSpace(user.ProfilePicture)
    ? $"{Guid.NewGuid()}{Path.GetExtension(model.UploadedPhoto.FileName)}"
    : user.ProfilePicture;
          var fullPath = Path.Combine(uploadsPath, fileName);

          using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
          {
            await model.UploadedPhoto.CopyToAsync(stream);
          }

          user.ProfilePicture = fileName;
        }
      }

      // DİĞER ALANLAR
      user.FirstName = model.FirstName;
      user.LastName = model.LastName;
      user.PhoneNumber = model.PhoneNumber;

      if (user.Email != model.Email)
      {
        user.Email = model.Email;
        user.UserName = model.Email;
        user.NormalizedEmail = model.Email.ToUpper();
        user.NormalizedUserName = model.Email.ToUpper();
      }

      var result = await _userManager.UpdateAsync(user);

      if (result.Succeeded)
      {
        HttpContext.Session.SetString("UserFullName", user.FirstName + " " + user.LastName);
        HttpContext.Session.SetString("ProfilePicture", user.ProfilePicture ?? "1.png");
        HttpContext.Session.SetString("PhoneNumber", user.PhoneNumber ?? "");
        HttpContext.Session.SetString("Email", user.Email ?? "");
        HttpContext.Session.SetString("UnitName", user.Unit?.Unit ?? "Tanımsız");
        TempData["SuccessMessage"] = "Profil bilgileri güncellendi.";
        return RedirectToAction("Profile");
      }

      ModelState.AddModelError(string.Empty, "Güncelleme başarısız.");
      return View("Profile", model);
    }


    [HttpPost]
    public async Task<IActionResult> ChangePassword(ProfileViewModel model)
    {
      if (model.NewPassword != model.ConfirmPassword)
      {
        ModelState.AddModelError("ConfirmPassword", "Yeni şifreler eşleşmiyor.");
        return View("Profile", model);
      }

      var user = await _userManager.GetUserAsync(User);

      if (user == null)
      {
        return RedirectToAction("Login", "Account");
      }

      var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

      if (result.Succeeded)
      {
        await _signInManager.RefreshSignInAsync(user);
        TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi.";
        return RedirectToAction("Profile");
      }

      foreach (var error in result.Errors)
      {
        ModelState.AddModelError(string.Empty, error.Description);
      }

      return View("Profile", model);
    }
  }
}
