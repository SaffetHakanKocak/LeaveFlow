using LeaveFlow.Application.Abstractions.LeaveRequests;
using LeaveFlow.Application.Authorization;
using LeaveFlow.Application.Identity;
using LeaveFlow.Application.LeaveRequests;
using LeaveFlow.Application.People;
using LeaveFlow.Domain.LeaveRequests;
using LeaveFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveFlow.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.RequireConsultant)]
public sealed class LeaveRequestsController(ILeaveRequestService leaveRequestService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        string? status,
        DateOnly? fromDate,
        DateOnly? toDate,
        int pageNumber = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var results = await leaveRequestService.GetMineAsync(
            User.GetRequiredUserId(),
            new LeaveRequestSearchRequest(status, fromDate, toDate, pageNumber, pageSize),
            cancellationToken);

        if (results is null)
        {
            return Forbid();
        }

        return View(new MyLeaveRequestsViewModel
        {
            Status = status,
            FromDate = fromDate,
            ToDate = toDate,
            Results = results
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var leaveRequest = await leaveRequestService.GetMineByIdAsync(
            User.GetRequiredUserId(),
            id,
            cancellationToken);

        return leaveRequest is null ? Forbid() : View(leaveRequest);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new LeaveRequestFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeaveRequestFormViewModel model, CancellationToken cancellationToken)
    {
        var input = ToInput(model);
        if (!ModelState.IsValid || !AddValidationErrors(leaveRequestService.Validate(input)))
        {
            return View(model);
        }

        var result = await leaveRequestService.CreateMineAsync(User.GetRequiredUserId(), input, cancellationToken);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Details), new { id = result.LeaveRequestId });
        }

        AddCreateError(result.ErrorCode);
        return View(model);
    }

    private void AddCreateError(string? errorCode)
    {
        var message = errorCode switch
        {
            "InvalidConsultant" => "İzin talebi oluşturmak için aktif bir danışman profili gerekir.",
            "Overlap" => "Bu talep mevcut bekleyen veya onaylı bir izin talebiyle çakışıyor.",
            _ => "İzin talebi oluşturulamadı."
        };

        ModelState.AddModelError(string.Empty, message);
    }

    private bool AddValidationErrors(ValidationResult validation)
    {
        foreach (var error in validation.Errors)
        {
            foreach (var message in error.Value)
            {
                ModelState.AddModelError(error.Key, message);
            }
        }

        return validation.IsValid;
    }

    private static LeaveRequestInput ToInput(LeaveRequestFormViewModel model)
    {
        return new LeaveRequestInput(model.Reason, model.StartDate, model.EndDate);
    }

    public static string StatusBadgeClass(string status)
    {
        return status switch
        {
            LeaveRequestStatuses.Pending => "text-bg-warning",
            LeaveRequestStatuses.Approved => "text-bg-success",
            LeaveRequestStatuses.Rejected => "text-bg-danger",
            _ => "text-bg-secondary"
        };
    }
}
