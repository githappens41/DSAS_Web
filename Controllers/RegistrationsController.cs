using DSAS.Web.Models;
using DSAS.Web.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DSAS.Web.Controllers;

public class RegistrationsController : StaffControllerBase
{
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IConfiguration _configuration;

    public RegistrationsController(IRegistrationRepository registrationRepository, IConfiguration configuration)
    {
        _registrationRepository = registrationRepository;
        _configuration = configuration;
    }

    // Single combined screen: view (Full Name, Student Number, Programme,
    // submitted photo, proof of registration, current status) PLUS
    // Approve/Reject. Staff can view and update status, but there is no
    // separate "edit the applicant's data" screen anywhere -- the
    // Dashboard's "Edit" link is intentionally inert (see Dashboard view).
    public async Task<IActionResult> Details(string id)
    {
        if (RequireToken() is IActionResult redirect) return redirect;

        var registration = await _registrationRepository.GetByIdAsync(AccessToken!, id);
        if (registration is null) return NotFound();

        // Pass the Supabase URL so the view can construct the file URLs
        ViewBag.SupabaseUrl = _configuration["Supabase:Url"];

        return View(new RegistrationDetailViewModel { Registration = registration });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(string id)
    {
        if (RequireToken() is IActionResult redirect) return redirect;

        await _registrationRepository.UpdateStatusAsync(AccessToken!, id, "Approved", null, StaffId);
        return RedirectToAction(nameof(Details), new { id });
    }

    // One action covers both "reject" and "request resubmission" --
    // same status ("Rejected"), the reason text is what tells the
    // student which one it was.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(string id, RejectViewModel model)
    {
        if (RequireToken() is IActionResult redirect) return redirect;

        if (!ModelState.IsValid)
        {
            var registration = await _registrationRepository.GetByIdAsync(AccessToken!, id);
            if (registration is null) return NotFound();

            // Pass the Supabase URL back to the view in case of validation error
            ViewBag.SupabaseUrl = _configuration["Supabase:Url"];

            return View("Details", new RegistrationDetailViewModel
            {
                Registration = registration,
                ErrorMessage = "Please provide a reason for rejection."
            });
        }

        await _registrationRepository.UpdateStatusAsync(AccessToken!, id, "Rejected", model.Reason, StaffId);

        // THIS IS THE POP-UP MESSAGE
        TempData["SuccessMessage"] = "Application rejected successfully.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (RequireToken() is IActionResult redirect) return redirect;

        await _registrationRepository.DeleteAsync(AccessToken!, id);
        return RedirectToAction("Index", "Dashboard");
    }
}