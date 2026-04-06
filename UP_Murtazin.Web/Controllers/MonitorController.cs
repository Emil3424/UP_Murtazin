using Microsoft.AspNetCore.Mvc;
using UP_Murtazin.Web.Models;
using UP_Murtazin.Web.Services;

namespace UP_Murtazin.Web.Controllers;

public class MonitorController : Controller
{
    private const string UserSessionKey = "CurrentUser";
    private readonly SqlDataService _dataService;

    public MonitorController(SqlDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string sortBy = "number", string search = "", bool statusWorking = false, bool statusBroken = false, bool statusMaintenance = false)
    {
        if (HttpContext.Session.GetString(UserSessionKey) is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var machines = await _dataService.GetMonitorMachinesAsync();

        if (statusWorking || statusBroken || statusMaintenance)
        {
            var allowed = new List<string>();
            if (statusWorking) allowed.Add("Работает");
            if (statusBroken) allowed.Add("Сломан");
            if (statusMaintenance) allowed.Add("Обслуживается");
            machines = machines.Where(m => allowed.Contains(m.Status)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            machines = machines
                .Where(m => m.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || m.Model.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || m.Address.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        machines = sortBy switch
        {
            "name" => machines.OrderBy(m => m.Name).ToList(),
            "money" => machines.OrderByDescending(m => m.TotalIncome).ToList(),
            "load" => machines.OrderByDescending(m => m.LoadPercentage).ToList(),
            "status" => machines.OrderBy(m => m.Status).ToList(),
            _ => machines.OrderBy(m => m.Number).ToList()
        };

        var model = new MonitorViewModel
        {
            SortBy = sortBy,
            Search = search,
            StatusWorking = statusWorking,
            StatusBroken = statusBroken,
            StatusMaintenance = statusMaintenance,
            Machines = machines
        };

        return View(model);
    }
}
