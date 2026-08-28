using System.ComponentModel.DataAnnotations;

namespace LeaveFlow.Web.Models;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Bu alan zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    [MaxLength(256)]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bu alan zorunludur.")]
    [DataType(DataType.Password)]
    [MaxLength(256)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
