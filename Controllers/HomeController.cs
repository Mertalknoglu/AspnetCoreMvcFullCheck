using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalepYonetim.Models.ViewModel;

namespace TalepYonetim.Controllers
{
  [Authorize]
  public class HomeController : Controller
  {
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
      _context = context;
    }

    #region TalepList
    public IActionResult Index()
    {


      return View();
    }
    #endregion
    public IActionResult Privacy()
    {
      return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

  }
}
