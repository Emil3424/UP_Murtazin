namespace UP_Murtazin.Web.Models;

public sealed class MonitorViewModel
{
    public string SortBy { get; set; } = "number";
    public string Search { get; set; } = string.Empty;
    public bool StatusWorking { get; set; }
    public bool StatusBroken { get; set; }
    public bool StatusMaintenance { get; set; }
    public List<MonitorMachineViewModel> Machines { get; set; } = [];
}

public sealed class MonitorMachineViewModel
{
    public int Number { get; set; }
    public string MachineId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Model { get; set; } = "Not specified";
    public string Address { get; set; } = "Not specified";
    public string Location { get; set; } = "Not specified";
    public string OperatorName { get; set; } = "Not specified";
    public string Status { get; set; } = "Unknown";
    public decimal TotalIncome { get; set; }
    public int LoadPercentage { get; set; }
}
