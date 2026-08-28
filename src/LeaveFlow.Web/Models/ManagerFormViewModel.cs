using System.ComponentModel.DataAnnotations;

namespace LeaveFlow.Web.Models;

public sealed class ManagerFormViewModel
{
    public Guid? Id { get; init; }

    [Required(ErrorMessage = "Bu alan zorunludur.")]
    [StringLength(80, ErrorMessage = "Bu alan en fazla 80 karakter olabilir.")]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bu alan zorunludur.")]
    [StringLength(80, ErrorMessage = "Bu alan en fazla 80 karakter olabilir.")]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bu alan zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
    [StringLength(256, ErrorMessage = "Bu alan en fazla 256 karakter olabilir.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [StringLength(120, ErrorMessage = "Bu alan en fazla 120 karakter olabilir.")]
    [Display(Name = "Departman")]
    public string? Department { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;
}
