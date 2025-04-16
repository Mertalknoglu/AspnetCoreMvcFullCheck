using Microsoft.AspNetCore.Mvc;

public class ErrorController : Controller
{
  [Route("Error/500")]
  public IActionResult Error500()
  {
    return View("ServerError");
  }

  [Route("Error/{code}")]
  public IActionResult HandleStatusCode(int code)
  {
    return code == 404 ? View("NotFound") : View("GeneralError");
  }

}
