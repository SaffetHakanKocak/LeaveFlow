using System.ComponentModel.DataAnnotations;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.Holidays;

namespace LeaveFlow.Web.Models;

public sealed class OrganizationHolidayListViewModel
{
    public string? Search { get; init; }

    public bool? IsActive { get; init; }

    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public PagedResult<OrganizationHolidayListItem> Results { get; init; } = new([], 1, 25, 0);
}

public sealed class OfficialHolidayListViewModel
{
    public string? Search { get; init; }

    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public PagedResult<OfficialHolidayListItem> Results { get; init; } = new([], 1, 25, 0);
}

public sealed class OrganizationHolidayFormViewModel
{
    public Guid? Id { get; init; }

    [Required(ErrorMessage = "Bu alan zorunludur.")]
    [StringLength(160, ErrorMessage = "Bu alan en fazla 160 karakter olabilir.")]
    [Display(Name = "Ad")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Bu alan zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Başlangıç Tarihi")]
    public DateOnly? StartDate { get; init; }

    [Required(ErrorMessage = "Bu alan zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Bitiş Tarihi")]
    public DateOnly? EndDate { get; init; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; init; } = true;
}

public sealed class OfficialHolidayFormViewModel
{
    public Guid? Id { get; init; }

    [Required(ErrorMessage = "Bu alan zorunludur.")]
    [StringLength(160, ErrorMessage = "Bu alan en fazla 160 karakter olabilir.")]
    [Display(Name = "Ad")]
    public string Name { get; init; } = string.Empty;

    [Required(ErrorMessage = "Bu alan zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Başlangıç Tarihi")]
    public DateOnly? StartDate { get; init; }

    [Required(ErrorMessage = "Bu alan zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Bitiş Tarihi")]
    public DateOnly? EndDate { get; init; }
}
