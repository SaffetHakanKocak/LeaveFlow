using System.ComponentModel.DataAnnotations;

namespace LeaveFlow.Web.Models;

public sealed class ManagerFormViewModel
{
    public Guid? Id { get; init; }

    [Required]
    [StringLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Department { get; set; }

    public bool IsActive { get; set; } = true;
}
