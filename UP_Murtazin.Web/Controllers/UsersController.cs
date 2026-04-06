using Microsoft.AspNetCore.Mvc;
using UP_Murtazin.Web.Models;
using UP_Murtazin.Web.Services;

namespace UP_Murtazin.Web.Controllers;

public class UsersController : Controller
{
    private const string UserSessionKey = "CurrentUser";
    private readonly SqlDataService _dataService;

    public UsersController(SqlDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString(UserSessionKey) is null)
        {
            return RedirectToAction("Login", "Account");
        }
        return View(await _dataService.GetUsersAsync());
    }

    [HttpGet]
    public IActionResult Create() => View("Edit", new UserViewModel());

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _dataService.GetUserAsync(id);
        return user is null ? NotFound() : View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _dataService.SaveUserAsync(model);
        return RedirectToAction(nameof(Index));
    }
}
