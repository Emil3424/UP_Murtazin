namespace UP_Murtazin.Web.Models;

public sealed class HomeDashboardViewModel
{
    public DateTime LastUpdate { get; set; }
    public int TotalMachines { get; set; }
    public int WorkingMachines { get; set; }
    public int MaintenanceMachines { get; set; }
    public int BrokenMachines { get; set; }
    public decimal MoneyInMachines { get; set; }
    public decimal ChangeInMachines { get; set; }
    public decimal RevenueToday { get; set; }
    public decimal RevenueYesterday { get; set; }
    public decimal CollectedToday { get; set; }
    public decimal CollectedYesterday { get; set; }
    public int ServicedToday { get; set; }
    public int ServicedYesterday { get; set; }
    public List<SalesPointViewModel> Last10DaysSales { get; set; } = [];
    public List<NewsItemViewModel> News { get; set; } = [];
}

public sealed class SalesPointViewModel
{
    public DateTime Date { get; set; }
    public decimal TotalSum { get; set; }
    public decimal TotalQuantity { get; set; }
}

public sealed class NewsItemViewModel
{
    public string Date { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
