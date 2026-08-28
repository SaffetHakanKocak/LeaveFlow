using LeaveFlow.Application.Calendar;

namespace LeaveFlow.Web.Models;

public sealed class OrganizationCalendarViewModel
{
    public DateOnly? StartDate { get; init; }

    public DateOnly? EndDate { get; init; }

    public string ViewMode { get; init; } = OrganizationCalendarDateRange.MonthView;

    public Guid? ManagerId { get; init; }

    public Guid? ConsultantId { get; init; }

    public OrganizationCalendarResult? Calendar { get; init; }

    public DateOnly PreviousStartDate { get; init; }

    public DateOnly PreviousEndDate { get; init; }

    public DateOnly NextStartDate { get; init; }

    public DateOnly NextEndDate { get; init; }
}

public sealed class OrganizationCalendarDetailViewModel
{
    public required CalendarEventDetail Event { get; init; }

    public required string ReturnUrl { get; init; }
}
