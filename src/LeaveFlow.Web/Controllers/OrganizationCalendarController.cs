using LeaveFlow.Application.Abstractions.Calendar;
using LeaveFlow.Application.Calendar;
using LeaveFlow.Application.Identity;
using LeaveFlow.Application.People;
using LeaveFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveFlow.Web.Controllers;

[Authorize]
public sealed class OrganizationCalendarController(IOrganizationCalendarService calendarService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        DateOnly? startDate,
        DateOnly? endDate,
        string? viewMode,
        Guid? managerId,
        Guid? consultantId,
        CancellationToken cancellationToken = default)
    {
        var query = new OrganizationCalendarQuery(startDate, endDate, viewMode, managerId, consultantId);
        var validation = calendarService.Validate(query);
        if (!validation.IsValid)
        {
            AddValidationErrors(validation);
            return View(new OrganizationCalendarViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                ViewMode = NormalizeViewMode(viewMode),
                ManagerId = managerId,
                ConsultantId = consultantId
            });
        }

        var calendar = await calendarService.GetAsync(User.GetRequiredUserId(), query, cancellationToken);
        if (calendar is null)
        {
            return Forbid();
        }

        return View(ToViewModel(calendar));
    }

    [HttpGet]
    public async Task<IActionResult> Details(
        string eventType,
        Guid eventId,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var detail = await calendarService.GetDetailAsync(
            User.GetRequiredUserId(),
            new CalendarEventDetailQuery(eventType, eventId),
            cancellationToken);

        if (detail is null)
        {
            return Forbid();
        }

        return View(new OrganizationCalendarDetailViewModel
        {
            Event = detail,
            ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Action(nameof(Index))!
        });
    }

    private static OrganizationCalendarViewModel ToViewModel(OrganizationCalendarResult calendar)
    {
        var viewMode = calendar.Query.ViewMode ?? OrganizationCalendarDateRange.MonthView;
        var shiftMonths = viewMode == OrganizationCalendarDateRange.MonthView;
        var previousStart = shiftMonths
            ? calendar.Query.StartDate!.Value.AddMonths(-1)
            : calendar.Query.StartDate!.Value.AddDays(-7);
        var previousEnd = shiftMonths
            ? previousStart.AddMonths(1).AddDays(-1)
            : previousStart.AddDays(6);
        var nextStart = shiftMonths
            ? calendar.Query.StartDate!.Value.AddMonths(1)
            : calendar.Query.StartDate!.Value.AddDays(7);
        var nextEnd = shiftMonths
            ? nextStart.AddMonths(1).AddDays(-1)
            : nextStart.AddDays(6);

        return new OrganizationCalendarViewModel
        {
            StartDate = calendar.Query.StartDate,
            EndDate = calendar.Query.EndDate,
            ViewMode = viewMode,
            ManagerId = calendar.Query.ManagerId,
            ConsultantId = calendar.Query.ConsultantId,
            Calendar = calendar,
            PreviousStartDate = previousStart,
            PreviousEndDate = previousEnd,
            NextStartDate = nextStart,
            NextEndDate = nextEnd
        };
    }

    private void AddValidationErrors(ValidationResult validation)
    {
        foreach (var error in validation.Errors)
        {
            foreach (var message in error.Value)
            {
                ModelState.AddModelError(error.Key, message);
            }
        }
    }

    private static string NormalizeViewMode(string? viewMode)
    {
        return string.Equals(viewMode, OrganizationCalendarDateRange.WeekView, StringComparison.OrdinalIgnoreCase)
            ? OrganizationCalendarDateRange.WeekView
            : OrganizationCalendarDateRange.MonthView;
    }
}
