using Microsoft.AspNetCore.Mvc;

namespace DSAS.Web.Controllers;

// Public welcome page -- the first screen anyone sees.
public class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Error() => View();
}