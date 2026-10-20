using DSAS.Web.Models;
using DSAS.Web.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DSAS.Web.Controllers;

public class DashboardController : StaffControllerBase
{
    private readonly IRegistrationRepository _registrationRepository;

    public DashboardController(IRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<IActionResult> Index(string? search, string? status)
    {
        if (RequireToken() is IActionResult redirect) return redirect;

        var registrations = await _registrationRepository.GetAllAsync(AccessToken!, search, status);

        var allForCounts = string.IsNullOrWhiteSpace(search) && string.IsNullOrWhiteSpace(status)
            ? registrations
            : await _registrationRepository.GetAllAsync(AccessToken!);

        var model = new DashboardViewModel
        {
            Registrations = registrations,
            SearchTerm = search,
            StatusFilter = status,
            Submitted = allForCounts.Count,
            Pending = allForCounts.Count(r => r.Status == "Pending"),
            Approved = allForCounts.Count(r => r.Status == "Approved"),
            Rejected = allForCounts.Count(r => r.Status == "Rejected"),
        };

        return View(model);
    }
}