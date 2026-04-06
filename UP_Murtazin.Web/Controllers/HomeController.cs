using Microsoft.AspNetCore.Mvc;
using UP_Murtazin.Web.Services;

namespace UP_Murtazin.Web.Controllers;

public class HomeController : Controller
{
    private readonly SqlDataService _dataService;
    private const string UserSessionKey = "CurrentUser";

    public HomeController(SqlDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString(UserSessionKey) is null)
        {
            return RedirectToAction("Login", "Account");
        }

        return View(await _dataService.GetHomeDashboardAsync());
    }
}
