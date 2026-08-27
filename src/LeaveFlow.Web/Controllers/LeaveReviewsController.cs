using LeaveFlow.Application.Abstractions.LeaveRequests;
using LeaveFlow.Application.Identity;
using LeaveFlow.Application.LeaveRequests;
using LeaveFlow.Application.People;
using LeaveFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveFlow.Web.Controllers;

[Authorize]
public sealed class LeaveReviewsController(ILeaveReviewService reviewService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        Guid? consultantId,
        DateOnly? fromDate,
        DateOnly? toDate,
        string? status,
        int pageNumber = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var results = await reviewService.GetPendingAsync(
            User.GetRequiredUserId(),
            new LeaveReviewSearchRequest(consultantId, fromDate, toDate, status, pageNumber, pageSize),
            cancellationToken);

        if (results is null)
        {
            return Forbid();
        }

        return View(new PendingLeaveRequestsViewModel
        {
            ConsultantId = consultantId,
            Status = status,
            FromDate = fromDate,
            ToDate = toDate,
            Results = results
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var model = await BuildDetailModelAsync(id, cancellationToken);
        return model is null ? Forbid() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id, ReviewDecisionViewModel model, CancellationToken cancellationToken)
    {
        var input = new ReviewDecisionInput(model.ReviewNote);
        if (!AddValidationErrors(reviewService.Validate(input)))
        {
            var detailModel = await BuildDetailModelAsync(id, cancellationToken);
            if (detailModel is null)
            {
                return Forbid();
            }

            return View(nameof(Details), detailModel);
        }

        var result = await reviewService.ApproveAsync(User.GetRequiredUserId(), id, input, cancellationToken);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        AddDecisionError(result.ErrorCode);
        var failedModel = await BuildDetailModelAsync(id, cancellationToken);
        return failedModel is null ? Forbid() : View(nameof(Details), failedModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id, ReviewDecisionViewModel model, CancellationToken cancellationToken)
    {
        var input = new ReviewDecisionInput(model.ReviewNote);
        if (!AddValidationErrors(reviewService.Validate(input)))
        {
            var detailModel = await BuildDetailModelAsync(id, cancellationToken);
            if (detailModel is null)
            {
                return Forbid();
            }

            return View(nameof(Details), detailModel);
        }

        var result = await reviewService.RejectAsync(User.GetRequiredUserId(), id, input, cancellationToken);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        AddDecisionError(result.ErrorCode);
        var failedModel = await BuildDetailModelAsync(id, cancellationToken);
        return failedModel is null ? Forbid() : View(nameof(Details), failedModel);
    }

    private async Task<LeaveReviewDetailViewModel?> BuildDetailModelAsync(Guid id, CancellationToken cancellationToken)
    {
        var request = await reviewService.GetForReviewAsync(User.GetRequiredUserId(), id, cancellationToken);
        if (request is null)
        {
            return null;
        }

        var conflicts = await reviewService.GetConflictsAsync(User.GetRequiredUserId(), id, cancellationToken) ?? [];
        return new LeaveReviewDetailViewModel
        {
            Request = request,
            Conflicts = conflicts
        };
    }

    private void AddDecisionError(string? errorCode)
    {
        var message = errorCode switch
        {
            "Unauthorized" => "You are not authorized to review this request.",
            "NotPendingOrUnauthorized" => "This request is no longer pending or cannot be reviewed by you.",
            _ => "The review decision could not be applied."
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
}
