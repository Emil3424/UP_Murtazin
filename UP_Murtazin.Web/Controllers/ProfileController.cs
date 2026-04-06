using Microsoft.AspNetCore.Mvc;
using UP_Murtazin.Web.Extensions;
using UP_Murtazin.Web.Models;
using UP_Murtazin.Web.Services;

namespace UP_Murtazin.Web.Controllers;

public class ProfileController : Controller
{
    private const string UserSessionKey = "CurrentUser";
    private readonly SqlDataService _dataService;

    public ProfileController(SqlDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<IActionResult> Index()
    {
        var sessionUser = HttpContext.Session.GetJson<UserSession>(UserSessionKey);
        if (sessionUser is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var profile = await _dataService.GetProfileAsync(sessionUser.UserId);
        if (profile is null)
        {
            profile = new ProfileViewModel
            {
                FullName = sessionUser.FullName,
                Email = sessionUser.Email,
                Role = sessionUser.Role,
                Phone = sessionUser.Phone,
                ImageBase64 = sessionUser.ImageBase64
            };
        }

        return View(profile);
    }
}
