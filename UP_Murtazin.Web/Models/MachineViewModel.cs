using System.ComponentModel.DataAnnotations;

namespace UP_Murtazin.Web.Models;

public sealed class MachineViewModel
{
    public string? VendingMachineId { get; set; }

    public double? SerialNumber { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Place { get; set; }
    public string? InstallDate { get; set; }
    public string? UserId { get; set; }
    public string? RfidCashCollection { get; set; }
    public string? RfidLoading { get; set; }
    public string? RfidService { get; set; }
    public string? Technician { get; set; }
}
