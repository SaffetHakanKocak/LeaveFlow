using LeaveFlow.Application.Calendar;

namespace LeaveFlow.Application.Abstractions.Calendar;

public interface IOrganizationCalendarRepository
{
    Task<IReadOnlyList<OrganizationCalendarDataRow>> GetForAdminAsync(
        OrganizationCalendarQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationCalendarDataRow>> GetForManagerAsync(
        Guid managerId,
        OrganizationCalendarQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationCalendarDataRow>> GetForConsultantAsync(
        Guid consultantId,
        OrganizationCalendarQuery query,
        CancellationToken cancellationToken = default);

    Task<CalendarEventDetail?> GetDetailForAdminAsync(
        CalendarEventDetailQuery query,
        CancellationToken cancellationToken = default);

    Task<CalendarEventDetail?> GetDetailForManagerAsync(
        Guid managerId,
        CalendarEventDetailQuery query,
        CancellationToken cancellationToken = default);

    Task<CalendarEventDetail?> GetDetailForConsultantAsync(
        Guid consultantId,
        CalendarEventDetailQuery query,
        CancellationToken cancellationToken = default);
}
