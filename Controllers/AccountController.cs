using System.Security.Claims;
using DSAS.Web.Models;
using DSAS.Web.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace DSAS.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthRepository _authRepository;
    private readonly IStaffRepository _staffRepository;

    public const string SessionAccessTokenKey = "SupabaseAccessToken";

    public AccountController(IAuthRepository authRepository, IStaffRepository staffRepository)
    {
        _authRepository = authRepository;
        _staffRepository = staffRepository;
    }

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _authRepository.SignInAsync(model.Email, model.Password);
        if (!result.Success || result.AccessToken is null || result.UserId is null)
        {
            model.ErrorMessage = result.Error ?? "Invalid email or password.";
            return View(model);
        }

        var staff = await _staffRepository.GetByIdAsync(result.AccessToken, result.UserId);
        if (staff is null)
        {
            // No staff row yet -- either this is their first login right
            // after confirming their email (Register couldn't create it
            // earlier, since confirmation hadn't happened yet), or this
            // account somehow exists in auth.users without ever going
            // through our Register page. Since Register is the only path
            // that creates an account here, it's safe to create the
            // profile now rather than reject a legitimate first login.
            await _staffRepository.InsertAsync(result.AccessToken, result.UserId, model.Email);
        }

        await SignInStaffAsync(result.UserId, model.Email, result.AccessToken);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _authRepository.SignUpAsync(model.Email, model.Password);
        if (!result.Success)
        {
            model.ErrorMessage = result.Error ?? "Could not create account.";
            return View(model);
        }

        if (result.AccessToken is null || result.UserId is null)
        {
            // "Confirm email" is ON -- the account genuinely was created
            // in Supabase, but can't sign in (and so can't create its own
            // staff row yet) until the confirmation link is clicked. This
            // is success, not failure -- do not show an error here.
            TempData["Info"] = "Account created. Please check your email to confirm it, then sign in.";
            return RedirectToAction(nameof(Login));
        }

        // Save the staff profile now, so it's there next time they log in.
        await _staffRepository.InsertAsync(result.AccessToken, result.UserId, model.Email);

        await SignInStaffAsync(result.UserId, model.Email, result.AccessToken);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private async Task SignInStaffAsync(string staffId, string email, string accessToken)
    {
        HttpContext.Session.SetString(SessionAccessTokenKey, accessToken);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, staffId),
            new(ClaimTypes.Email, email),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}