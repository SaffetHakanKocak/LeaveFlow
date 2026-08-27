using System.ComponentModel.DataAnnotations;
using LeaveFlow.Application.Common;
using LeaveFlow.Application.LeaveRequests;

namespace LeaveFlow.Web.Models;

public sealed class MyLeaveRequestsViewModel
{
    public string? Status { get; init; }

    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public PagedResult<LeaveRequestListItem> Results { get; init; } = new([], 1, 25, 0);
}

public sealed class LeaveRequestFormViewModel
{
    [Required]
    [StringLength(512)]
    public string Reason { get; init; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateOnly? StartDate { get; init; }

    [Required]
    [DataType(DataType.Date)]
    public DateOnly? EndDate { get; init; }
}
