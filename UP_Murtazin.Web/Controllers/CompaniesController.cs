using Microsoft.AspNetCore.Mvc;
using UP_Murtazin.Web.Services;

namespace UP_Murtazin.Web.Controllers;

public class CompaniesController : Controller
{
    private const string UserSessionKey = "CurrentUser";
    private readonly SqlDataService _dataService;

    public CompaniesController(SqlDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString(UserSessionKey) is null)
        {
            return RedirectToAction("Login", "Account");
        }
        var companies = await _dataService.GetCompaniesAsync();
        return View(companies);
    }
}
