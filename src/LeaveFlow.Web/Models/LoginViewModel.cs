using System.ComponentModel.DataAnnotations;

namespace LeaveFlow.Web.Models;

public sealed class LoginViewModel
{
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MaxLength(256)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
