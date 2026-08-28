using LeaveFlow.Application.Calendar;
using LeaveFlow.Application.People;

namespace LeaveFlow.Application.Abstractions.Calendar;

public interface IOrganizationCalendarService
{
    Task<OrganizationCalendarResult?> GetAsync(
        Guid actorUserId,
        OrganizationCalendarQuery query,
        CancellationToken cancellationToken = default);

    Task<CalendarEventDetail?> GetDetailAsync(
        Guid actorUserId,
        CalendarEventDetailQuery query,
        CancellationToken cancellationToken = default);

    ValidationResult Validate(OrganizationCalendarQuery query);
}
