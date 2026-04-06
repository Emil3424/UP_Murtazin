namespace UP_Murtazin.Web.Models;

public sealed class ProfileViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Phone { get; set; } = "Not specified";
    public string? ImageBase64 { get; set; }
}
