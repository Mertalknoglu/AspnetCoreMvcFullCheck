using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AspnetCoreMvcFull.Filters
{
  public class AdminOnlyAttribute : ActionFilterAttribute
  {
    public override void OnActionExecuting(ActionExecutingContext context)
    {
      var httpContext = context.HttpContext;

      // 1. Oturum açık mı kontrol et
      if (!httpContext.User.Identity.IsAuthenticated)
      {
        // Giriş yapılmamışsa anasayfaya yönlendir
        context.Result = new RedirectToActionResult("Index", "Home", null);
        return;
      }

      // 2. Admin yetkisi var mı?
      var isAdmin = httpContext.Session.GetInt32("IsAdmin");

      if (isAdmin != 1)
      {
        // Giriş var ama admin değilse Request ekranına yönlendir
        context.Result = new RedirectToActionResult("Index", "Request", null);
        return;
      }

      // Admin ise devam etsin
    }
  }
}
