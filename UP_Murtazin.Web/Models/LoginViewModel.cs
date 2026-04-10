using System.ComponentModel.DataAnnotations;

namespace UP_Murtazin.Web.Models;

public sealed class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string CaptchaAnswer { get; set; } = string.Empty;

    public string CaptchaExpression { get; set; } = string.Empty;
}
