using Microsoft.AspNetCore.Mvc;
using UP_Murtazin.Web.Services;

namespace UP_Murtazin.Web.Controllers;

public class MachinesController : Controller
{
    private const string UserSessionKey = "CurrentUser";
    private readonly SqlDataService _dataService;

    public MachinesController(SqlDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString(UserSessionKey) is null)
        {
            return RedirectToAction("Login", "Account");
        }
        var machines = await _dataService.GetMachinesAsync();
        return View(machines);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View("Edit", new Models.MachineViewModel());
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var machine = await _dataService.GetMachineAsync(id);
        return machine is null ? NotFound() : View(machine);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Models.MachineViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _dataService.SaveMachineAsync(model);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        await _dataService.DeleteMachineAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
