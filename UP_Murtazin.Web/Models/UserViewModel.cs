using System.ComponentModel.DataAnnotations;

namespace UP_Murtazin.Web.Models;

public sealed class UserViewModel
{
    public string? UserId { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public string? Role { get; set; }
    public bool IsManager { get; set; }
    public bool IsEngineer { get; set; }
    public bool IsOperator { get; set; }
}
