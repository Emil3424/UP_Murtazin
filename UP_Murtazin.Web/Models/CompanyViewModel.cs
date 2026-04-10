using System.ComponentModel.DataAnnotations;

namespace UP_Murtazin.Web.Models;

public sealed class CompanyViewModel
{
    [Required]
    public string Company { get; set; } = string.Empty;
}
