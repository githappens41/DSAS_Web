using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSAS.Web.Controllers;

[Authorize]
public abstract class StaffControllerBase : Controller
{
    protected string? AccessToken => HttpContext.Session.GetString(AccountController.SessionAccessTokenKey);

    protected string StaffId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    protected IActionResult? RequireToken()
    {
        return string.IsNullOrEmpty(AccessToken)
            ? RedirectToAction("Login", "Account")
            : null;
    }
}