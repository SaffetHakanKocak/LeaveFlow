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

    [Required]
    [StringLength(160)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateOnly? StartDate { get; init; }

    [Required]
    [DataType(DataType.Date)]
    public DateOnly? EndDate { get; init; }

    public bool IsActive { get; init; } = true;
}

public sealed class OfficialHolidayFormViewModel
{
    public Guid? Id { get; init; }

    [Required]
    [StringLength(160)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateOnly? StartDate { get; init; }

    [Required]
    [DataType(DataType.Date)]
    public DateOnly? EndDate { get; init; }
}
