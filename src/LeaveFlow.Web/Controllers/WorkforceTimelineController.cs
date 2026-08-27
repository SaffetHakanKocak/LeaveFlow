using LeaveFlow.Application.Abstractions.Timeline;
using LeaveFlow.Application.Identity;
using LeaveFlow.Application.Timeline;
using LeaveFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveFlow.Web.Controllers;

[Authorize]
public sealed class WorkforceTimelineController(IWorkforceTimelineService timelineService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        DateOnly? startDate,
        DateOnly? endDate,
        Guid? managerId,
        string? consultantSearch,
        bool includeInactive = false,
        int pageNumber = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var query = new WorkforceTimelineQuery(
            startDate,
            endDate,
            managerId,
            consultantSearch,
            includeInactive,
            pageNumber,
            pageSize);

        var validation = timelineService.Validate(query);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                foreach (var message in error.Value)
                {
                    ModelState.AddModelError(error.Key, message);
                }
            }

            return View(new WorkforceTimelineViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                ManagerId = managerId,
                ConsultantSearch = consultantSearch,
                IncludeInactive = includeInactive
            });
        }

        var timeline = await timelineService.GetAsync(User.GetRequiredUserId(), query, cancellationToken);
        if (timeline is null)
        {
            return Forbid();
        }

        return View(new WorkforceTimelineViewModel
        {
            StartDate = timeline.Query.StartDate,
            EndDate = timeline.Query.EndDate,
            ManagerId = timeline.Query.ManagerId,
            ConsultantSearch = timeline.Query.ConsultantSearch,
            IncludeInactive = timeline.Query.IncludeInactive,
            Timeline = timeline
        });
    }
}
