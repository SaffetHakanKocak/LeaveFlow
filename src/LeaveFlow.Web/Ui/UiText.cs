using System.Globalization;
using LeaveFlow.Application.Calendar;
using LeaveFlow.Domain.LeaveRequests;

namespace LeaveFlow.Web.Ui;

public static class UiText
{
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    public static string Status(string status)
    {
        return status switch
        {
            LeaveRequestStatuses.Pending => "Bekliyor",
            LeaveRequestStatuses.Approved => "Onaylandı",
            LeaveRequestStatuses.Rejected => "Reddedildi",
            _ => status
        };
    }

    public static string Active(bool isActive)
    {
        return isActive ? "Aktif" : "Pasif";
    }

    public static string Scope(string scope)
    {
        return scope switch
        {
            "Administrator" => "Yönetici",
            "Manager" => "Yönetici",
            "Consultant" => "Danışman",
            _ => scope
        };
    }

    public static string Date(DateOnly date)
    {
        return date.ToString("dd.MM.yyyy", TurkishCulture);
    }

    public static string Date(DateOnly? date, string fallback = "-")
    {
        return date.HasValue ? Date(date.Value) : fallback;
    }

    public static string DateTime(DateTime date)
    {
        return date.ToString("dd.MM.yyyy HH:mm", TurkishCulture);
    }

    public static string DateTime(DateTime? date, string fallback = "-")
    {
        return date.HasValue ? DateTime(date.Value) : fallback;
    }

    public static string ShortDay(DateOnly date)
    {
        return date.ToString("ddd", TurkishCulture);
    }

    public static string ShortMonthDay(DateOnly date)
    {
        return date.ToString("dd.MM", TurkishCulture);
    }

    public static string EventType(string eventType)
    {
        return eventType switch
        {
            CalendarEventTypes.Leave => "İzin",
            CalendarEventTypes.OrganizationHoliday => "Kurum Tatili",
            CalendarEventTypes.OfficialHoliday => "Resmi Tatil",
            _ => "Etkinlik"
        };
    }

    public static string CompactEventType(string eventType)
    {
        return eventType switch
        {
            CalendarEventTypes.Leave => "İzin",
            CalendarEventTypes.OrganizationHoliday => "Kurum",
            CalendarEventTypes.OfficialHoliday => "Resmi",
            _ => "Etkinlik"
        };
    }
}
