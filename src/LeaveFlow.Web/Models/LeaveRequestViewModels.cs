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
    [Required(ErrorMessage = "Bu alan zorunludur.")]
    [StringLength(512, ErrorMessage = "Bu alan en fazla 512 karakter olabilir.")]
    [Display(Name = "Gerekçe")]
    public string Reason { get; init; } = string.Empty;

    [Required(ErrorMessage = "Bu alan zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Başlangıç Tarihi")]
    public DateOnly? StartDate { get; init; }

    [Required(ErrorMessage = "Bu alan zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Bitiş Tarihi")]
    public DateOnly? EndDate { get; init; }
}
