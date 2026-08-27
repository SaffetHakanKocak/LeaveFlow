using System.ComponentModel.DataAnnotations;

namespace LeaveFlow.Web.Models;

public sealed class ConsultantFormViewModel
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

    [StringLength(64)]
    public string? EmployeeNumber { get; set; }

    [StringLength(120)]
    public string? Department { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? StartDate { get; set; }

    public bool IsActive { get; set; } = true;
}
