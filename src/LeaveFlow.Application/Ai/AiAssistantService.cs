using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using LeaveFlow.Application.Abstractions.Ai;
using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LeaveFlow.Application.Ai;

public sealed class AiAssistantService(
    IAiChatClient chatClient,
    IAiToolRegistry toolRegistry,
    IAuditLogRepository auditLogRepository,
    IOptions<AiOptions> options,
    ILogger<AiAssistantService> logger,
    TimeProvider timeProvider) : IAiAssistantService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const int MaxToolRounds = 3;

    public async Task<AiAssistantResult> SendAsync(AiAssistantInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var settings = options.Value;
        if (!settings.Enabled)
        {
            return AiAssistantResult.Disabled();
        }

        if (input.UserId == Guid.Empty)
        {
            return AiAssistantResult.Failure("Kullanici oturumu dogrulanamadi.");
        }

        if (string.IsNullOrWhiteSpace(input.Prompt))
        {
            return AiAssistantResult.Failure("Mesaj alani zorunludur.");
        }

        if (input.Prompt.Length > settings.MaxPromptLength)
        {
            return AiAssistantResult.Failure($"Mesaj en fazla {settings.MaxPromptLength} karakter olabilir.");
        }

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(settings.TimeoutSeconds, 1, 60)));

            var localPlan = AiWorkforceQueryPlanner.CreatePlan(
                input.Prompt,
                DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime));
            if (localPlan.Status == AiWorkforceQueryPlanStatus.OutOfDomain)
            {
                return AiAssistantResult.Success("Bu asistan yalnizca LeaveFlow izin, ekip uygunlugu, tatil ve raporlama sorularini yanitlar.");
            }

            if (localPlan.Status == AiWorkforceQueryPlanStatus.NeedsClarification)
            {
                return AiAssistantResult.Success(localPlan.ClarificationMessage ?? "Lutfen tarih araligini veya kapsami netlestirin.");
            }

            if (localPlan.Status == AiWorkforceQueryPlanStatus.Ready)
            {
                return await ExecuteLocalPlanAsync(input, localPlan, timeout.Token);
            }

            var tools = toolRegistry.GetDefinitions();
            var toolResults = new List<AiToolResultMessage>();
            var usedTools = new List<string>();
            AiChatResponse response = new(string.Empty);

            for (var round = 0; round < MaxToolRounds; round++)
            {
                response = await chatClient.SendAsync(
                    new AiChatRequest(
                        input.UserId,
                        input.UserDisplayName,
                        input.Prompt.Trim(),
                        tools,
                        toolResults),
                    timeout.Token);

                if (response.ToolCalls is null || response.ToolCalls.Count == 0)
                {
                    break;
                }

                foreach (var toolCall in response.ToolCalls.Take(5))
                {
                    var result = await ExecuteToolCallAsync(
                        new AiToolExecutionContext(input.UserId, input.UserDisplayName),
                        toolCall,
                        timeout.Token);

                    usedTools.Add(toolCall.Name);
                    toolResults.Add(new AiToolResultMessage(
                        toolCall.Id,
                        toolCall.Name,
                        JsonSerializer.Serialize(result, JsonOptions),
                        result.Succeeded));
                }
            }

            return string.IsNullOrWhiteSpace(response.Message)
                ? AiAssistantResult.Failure("AI Asistan bos yanit dondurdu.")
                : AiAssistantResult.Success(response.Message.Trim(), usedTools.Distinct(StringComparer.Ordinal).ToArray());
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("AI assistant request timed out for user {UserId}.", input.UserId);
            return AiAssistantResult.Failure("AI Asistan zaman asimina ugradi. Lutfen tekrar deneyin.");
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "AI assistant request failed for user {UserId}.", input.UserId);
            return AiAssistantResult.Failure("AI Asistan su anda yanit veremiyor. Lutfen daha sonra tekrar deneyin.");
        }
    }

    private async Task<AiToolExecutionResult> ExecuteToolCallAsync(
        AiToolExecutionContext context,
        AiToolCall toolCall,
        CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        var succeeded = false;
        string? errorCode = null;

        try
        {
            if (!toolRegistry.TryGet(toolCall.Name, out var tool))
            {
                errorCode = "UnknownTool";
                return AiToolExecutionResult.Failure(errorCode);
            }

            var result = await tool.ExecuteAsync(context, toolCall.ArgumentsJson, cancellationToken);
            succeeded = result.Succeeded;
            errorCode = result.ErrorCode;
            return result;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "AI tool call failed for tool {ToolName} and user {UserId}.", toolCall.Name, context.UserId);
            errorCode = "ToolFailure";
            return AiToolExecutionResult.Failure(errorCode);
        }
        finally
        {
            var duration = (long)Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            await AuditToolCallAsync(context.UserId, toolCall.Name, succeeded, duration, errorCode, cancellationToken);
        }
    }

    private async Task<AiAssistantResult> ExecuteLocalPlanAsync(
        AiAssistantInput input,
        AiWorkforceQueryPlan plan,
        CancellationToken cancellationToken)
    {
        var context = new AiToolExecutionContext(input.UserId, input.UserDisplayName);
        var results = new List<AiPlannedToolResult>();

        foreach (var tool in plan.Tools)
        {
            var result = await ExecuteToolCallAsync(
                context,
                new AiToolCall(Guid.NewGuid().ToString("N"), tool.Name, tool.ArgumentsJson),
                cancellationToken);

            if (!result.Succeeded)
            {
                return AiAssistantResult.Success(FormatToolFailure(result.ErrorCode));
            }

            results.Add(new AiPlannedToolResult(tool.Name, JsonSerializer.SerializeToElement(result.Data, JsonOptions)));
        }

        return AiAssistantResult.Success(
            AiWorkforceQueryAnswerFormatter.Format(plan, results),
            results.Select(result => result.ToolName).Distinct(StringComparer.Ordinal).ToArray());
    }

    private static string FormatToolFailure(string? errorCode)
    {
        return errorCode switch
        {
            "Unauthorized" => "Bu soru mevcut yetki kapsaminizda yanitlanamiyor.",
            "InvalidDateRange" => "Tarih araligi desteklenen sinirin disinda.",
            "InvalidArguments" => "Bu soruyu yanitlamak icin tarih veya filtre bilgisi net degil.",
            _ => "Gerekli LeaveFlow verisi su anda alinamadi. Lutfen daha sonra tekrar deneyin."
        };
    }

    private async Task AuditToolCallAsync(
        Guid userId,
        string toolName,
        bool succeeded,
        long durationMilliseconds,
        string? errorCode,
        CancellationToken cancellationToken)
    {
        var metadata = new AiToolAuditMetadata(
            toolName,
            userId,
            DateTime.UtcNow,
            succeeded,
            durationMilliseconds,
            errorCode);

        await auditLogRepository.InsertAsync(
            new AuditLogRecord(
                userId,
                AuditActions.AiToolCall,
                "AiTool",
                toolName,
                succeeded ? AuditOutcomes.Success : AuditOutcomes.Failure,
                null,
                JsonSerializer.Serialize(metadata, JsonOptions)),
            cancellationToken);
    }
}

internal enum AiWorkforceQueryPlanStatus
{
    Unsupported,
    Ready,
    NeedsClarification,
    OutOfDomain
}

internal enum AiWorkforceQueryIntent
{
    UpcomingLeaves,
    TeamAvailability,
    LeaveConflicts,
    PeakLeaveDay,
    LeaveRequestStatusCounts,
    UpcomingHoliday,
    MyLeaveSummary,
    TeamLeaveSummary
}

internal sealed record AiWorkforceQueryPlan(
    AiWorkforceQueryPlanStatus Status,
    AiWorkforceQueryIntent? Intent,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyList<AiPlannedToolCall> Tools,
    string? ClarificationMessage = null)
{
    public static AiWorkforceQueryPlan Unsupported() => new(AiWorkforceQueryPlanStatus.Unsupported, null, null, null, []);

    public static AiWorkforceQueryPlan OutOfDomain() => new(AiWorkforceQueryPlanStatus.OutOfDomain, null, null, null, []);

    public static AiWorkforceQueryPlan Clarify(string message) => new(AiWorkforceQueryPlanStatus.NeedsClarification, null, null, null, [], message);
}

internal sealed record AiPlannedToolCall(string Name, string ArgumentsJson);

internal sealed record AiPlannedToolResult(string ToolName, JsonElement Data);

internal static partial class AiWorkforceQueryPlanner
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly string[] DomainTerms =
    [
        "izin", "müsait", "musait", "uygun", "takim", "takım", "ekip", "tatil",
        "resmi", "onay", "redd", "red", "çakış", "cakis", "yogun", "yoğun"
    ];

    public static AiWorkforceQueryPlan CreatePlan(string prompt, DateOnly today)
    {
        var normalized = Normalize(prompt);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return AiWorkforceQueryPlan.Unsupported();
        }

        if (!DomainTerms.Any(term => normalized.Contains(term, StringComparison.Ordinal)))
        {
            return AiWorkforceQueryPlan.OutOfDomain();
        }

        var range = ResolveDateRange(normalized, today);
        var mentionsDate = range is not null;

        if (ContainsAny(normalized, "resmi tatil", "tatil"))
        {
            range ??= (today, today.AddDays(60));
            return Ready(AiWorkforceQueryIntent.UpcomingHoliday, range.Value, "GetUpcomingHolidays");
        }

        if (ContainsAny(normalized, "kullanimimi", "kullanimim", "izin ozetim", "izinlerimi ozetle", "izin kullanımımı"))
        {
            range ??= (new DateOnly(today.Year, 1, 1), new DateOnly(today.Year, 12, 31));
            return Ready(AiWorkforceQueryIntent.MyLeaveSummary, range.Value, "GetMyLeaveRequests");
        }

        if (ContainsAny(normalized, "cakis", "çakış", "conflict"))
        {
            if (range is null)
            {
                return AiWorkforceQueryPlan.Clarify("Cakisma kontrolu icin tarih araligini belirtin.");
            }

            return Ready(AiWorkforceQueryIntent.LeaveConflicts, range.Value, "GetTeamAvailability");
        }

        if (ContainsAny(normalized, "musait", "müsait", "uygun", "availability"))
        {
            if (range is null)
            {
                return AiWorkforceQueryPlan.Clarify("Uygunluk sorusu icin tarih araligini belirtin.");
            }

            return Ready(AiWorkforceQueryIntent.TeamAvailability, range.Value, "GetTeamAvailability");
        }

        if (ContainsAny(normalized, "yogun", "yoğun", "peak"))
        {
            range ??= MonthRange(today);
            return Ready(AiWorkforceQueryIntent.PeakLeaveDay, range.Value, "GetOrganizationLeaveStatistics");
        }

        if (ContainsAny(normalized, "onaylandi", "onaylandı", "reddedildi", "reddedilen", "reddedil", "red edildi"))
        {
            range ??= MonthRange(today);
            return Ready(AiWorkforceQueryIntent.LeaveRequestStatusCounts, range.Value, "GetOrganizationLeaveStatistics");
        }

        if (ContainsAny(normalized, "takim", "takım", "ekip") && ContainsAny(normalized, "ozet", "özet", "durum"))
        {
            range ??= MonthRange(today);
            return Ready(AiWorkforceQueryIntent.TeamLeaveSummary, range.Value, "GetTeamAvailability", "GetOrganizationLeaveStatistics");
        }

        if (ContainsAny(normalized, "kimler", "izinli"))
        {
            if (range is null && !mentionsDate)
            {
                return AiWorkforceQueryPlan.Clarify("Kimlerin izinli oldugunu yanitlamak icin tarih araligini belirtin.");
            }

            return Ready(AiWorkforceQueryIntent.UpcomingLeaves, range!.Value, "GetUpcomingLeaves");
        }

        return AiWorkforceQueryPlan.Unsupported();
    }

    private static AiWorkforceQueryPlan Ready(AiWorkforceQueryIntent intent, (DateOnly Start, DateOnly End) range, params string[] tools)
    {
        var args = JsonSerializer.Serialize(new { startDate = range.Start, endDate = range.End }, JsonOptions);
        return new AiWorkforceQueryPlan(
            AiWorkforceQueryPlanStatus.Ready,
            intent,
            range.Start,
            range.End,
            tools.Select(tool => new AiPlannedToolCall(tool, args)).ToArray());
    }

    private static (DateOnly Start, DateOnly End)? ResolveDateRange(string normalized, DateOnly today)
    {
        if (normalized.Contains("yarin", StringComparison.Ordinal) || normalized.Contains("yarın", StringComparison.Ordinal))
        {
            var tomorrow = today.AddDays(1);
            return (tomorrow, tomorrow);
        }

        if (normalized.Contains("bugun", StringComparison.Ordinal) || normalized.Contains("bugün", StringComparison.Ordinal))
        {
            return (today, today);
        }

        if (normalized.Contains("onumuzdeki hafta", StringComparison.Ordinal) || normalized.Contains("önümüzdeki hafta", StringComparison.Ordinal))
        {
            var start = StartOfWeek(today).AddDays(7);
            return (start, start.AddDays(6));
        }

        if (normalized.Contains("bu hafta", StringComparison.Ordinal))
        {
            var start = StartOfWeek(today);
            return (start, start.AddDays(6));
        }

        if (normalized.Contains("gelecek ay", StringComparison.Ordinal))
        {
            return MonthRange(new DateOnly(today.Year, today.Month, 1).AddMonths(1));
        }

        if (normalized.Contains("bu ay", StringComparison.Ordinal))
        {
            return MonthRange(today);
        }

        if (normalized.Contains("bu yil", StringComparison.Ordinal) || normalized.Contains("bu yıl", StringComparison.Ordinal))
        {
            return (new DateOnly(today.Year, 1, 1), new DateOnly(today.Year, 12, 31));
        }

        var match = DayRangeRegex().Match(normalized);
        if (match.Success && TryParseTurkishMonth(match.Groups["month"].Value, out var month))
        {
            var startDay = int.Parse(match.Groups["start"].Value);
            var endDay = int.Parse(match.Groups["end"].Value);
            if (DateOnly.TryParse($"{today.Year}-{month:00}-{startDay:00}", out var start)
                && DateOnly.TryParse($"{today.Year}-{month:00}-{endDay:00}", out var end)
                && start <= end)
            {
                return (start, end);
            }
        }

        return null;
    }

    private static (DateOnly Start, DateOnly End) MonthRange(DateOnly date)
    {
        var start = new DateOnly(date.Year, date.Month, 1);
        return (start, start.AddMonths(1).AddDays(-1));
    }

    private static DateOnly StartOfWeek(DateOnly date)
    {
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-diff);
    }

    private static bool TryParseTurkishMonth(string value, out int month)
    {
        month = Normalize(value) switch
        {
            "ocak" => 1,
            "subat" => 2,
            "mart" => 3,
            "nisan" => 4,
            "mayis" => 5,
            "haziran" => 6,
            "temmuz" => 7,
            "agustos" => 8,
            "eylul" => 9,
            "ekim" => 10,
            "kasim" => 11,
            "aralik" => 12,
            _ => 0
        };

        return month > 0;
    }

    private static bool ContainsAny(string value, params string[] terms) =>
        terms.Any(term => value.Contains(Normalize(term), StringComparison.Ordinal));

    private static string Normalize(string value)
    {
        return value.Trim().ToLowerInvariant()
            .Replace('ı', 'i')
            .Replace('ğ', 'g')
            .Replace('ü', 'u')
            .Replace('ş', 's')
            .Replace('ö', 'o')
            .Replace('ç', 'c');
    }

    [GeneratedRegex(@"(?<start>\d{1,2})\s*[-/]\s*(?<end>\d{1,2})\s+(?<month>ocak|subat|şubat|mart|nisan|mayis|mayıs|haziran|temmuz|agustos|ağustos|eylul|eylül|ekim|kasim|kasım|aralik|aralık)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DayRangeRegex();
}

internal static class AiWorkforceQueryAnswerFormatter
{
    public static string Format(AiWorkforceQueryPlan plan, IReadOnlyList<AiPlannedToolResult> results)
    {
        var first = results[0].Data;
        return plan.Intent switch
        {
            AiWorkforceQueryIntent.UpcomingLeaves => FormatUpcomingLeaves(plan, first),
            AiWorkforceQueryIntent.TeamAvailability => FormatTeamAvailability(plan, first),
            AiWorkforceQueryIntent.LeaveConflicts => FormatConflicts(plan, first),
            AiWorkforceQueryIntent.PeakLeaveDay => FormatPeakDay(plan, first),
            AiWorkforceQueryIntent.LeaveRequestStatusCounts => FormatStatusCounts(plan, first),
            AiWorkforceQueryIntent.UpcomingHoliday => FormatUpcomingHoliday(first),
            AiWorkforceQueryIntent.MyLeaveSummary => FormatMyLeaveSummary(plan, first),
            AiWorkforceQueryIntent.TeamLeaveSummary => FormatTeamLeaveSummary(plan, results),
            _ => "Bu LeaveFlow sorusu icin uygun bir yanit hazirlanamadi."
        };
    }

    private static string FormatUpcomingLeaves(AiWorkforceQueryPlan plan, JsonElement data)
    {
        var leaves = GetArray(data, "leaves");
        if (leaves.Count == 0)
        {
            return RangeNoData(plan);
        }

        var names = leaves
            .Select(item => GetString(item, "consultantName"))
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return names.Length == 0
            ? RangeNoData(plan)
            : $"{FormatRange(plan)} izinli kisiler: {string.Join(", ", names)}.";
    }

    private static string FormatTeamAvailability(AiWorkforceQueryPlan plan, JsonElement data)
    {
        var consultants = GetArray(data, "consultants");
        if (consultants.Count == 0)
        {
            return RangeNoData(plan);
        }

        var onLeave = consultants.Count(item => GetArray(item, "leaveDays").Count > 0);
        var available = Math.Max(0, GetInt(data, "totalCount") - onLeave);
        return $"{FormatRange(plan)} ekipte {available} kisi musait, {onLeave} kisi izinli gorunuyor.";
    }

    private static string FormatConflicts(AiWorkforceQueryPlan plan, JsonElement data)
    {
        var dayCounts = GetArray(data, "consultants")
            .SelectMany(item => GetArray(item, "leaveDays"))
            .GroupBy(item => GetDate(item, "date"))
            .Where(group => group.Key is not null && group.Count() > 1)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .ToArray();

        if (dayCounts.Length == 0)
        {
            return $"{FormatRange(plan)} izin cakismasi bulunmadi.";
        }

        var top = dayCounts[0];
        return $"{FormatRange(plan)} izin cakismasi var. En yogun cakisma {top.Key:yyyy-MM-dd} tarihinde {top.Count()} kisi ile gorunuyor.";
    }

    private static string FormatPeakDay(AiWorkforceQueryPlan plan, JsonElement data)
    {
        var peakDays = GetArray(data, "peakLeaveDays");
        if (peakDays.Count == 0)
        {
            return RangeNoData(plan);
        }

        var top = peakDays
            .OrderByDescending(item => GetInt(item, "consultantCount"))
            .ThenBy(item => GetDate(item, "leaveDate"))
            .First();

        return $"{FormatRange(plan)} en yogun izin gunu {GetDate(top, "leaveDate"):yyyy-MM-dd}; {GetInt(top, "consultantCount")} kisi izinli.";
    }

    private static string FormatStatusCounts(AiWorkforceQueryPlan plan, JsonElement data)
    {
        var statuses = GetArray(data, "statusDistribution");
        if (statuses.Count == 0)
        {
            return RangeNoData(plan);
        }

        var approved = SumStatus(statuses, "Approved");
        var rejected = SumStatus(statuses, "Rejected");
        return $"{FormatRange(plan)} {approved} izin talebi onaylandi, {rejected} izin talebi reddedildi.";
    }

    private static string FormatUpcomingHoliday(JsonElement data)
    {
        var holidays = GetArray(data, "holidays");
        if (holidays.Count == 0)
        {
            return "Yaklasan resmi tatil kaydi bulunamadi.";
        }

        var next = holidays
            .OrderBy(item => GetDate(item, "startDate"))
            .First();

        return $"Yaklasan tatil {GetString(next, "name")} tarihinde basliyor: {GetDate(next, "startDate"):yyyy-MM-dd}.";
    }

    private static string FormatMyLeaveSummary(AiWorkforceQueryPlan plan, JsonElement data)
    {
        var requests = GetArray(data, "requests");
        if (requests.Count == 0)
        {
            return RangeNoData(plan);
        }

        var approved = requests.Count(item => IsStatus(item, "Approved"));
        var pending = requests.Count(item => IsStatus(item, "Pending"));
        var rejected = requests.Count(item => IsStatus(item, "Rejected"));
        return $"{FormatRange(plan)} kendi izin talepleriniz: {approved} onayli, {pending} bekleyen, {rejected} reddedilmis.";
    }

    private static string FormatTeamLeaveSummary(AiWorkforceQueryPlan plan, IReadOnlyList<AiPlannedToolResult> results)
    {
        var availability = results.First(result => result.ToolName == "GetTeamAvailability").Data;
        var statistics = results.First(result => result.ToolName == "GetOrganizationLeaveStatistics").Data;
        var consultants = GetArray(availability, "consultants");
        var onLeave = consultants.Count(item => GetArray(item, "leaveDays").Count > 0);
        var approvedDays = GetArray(statistics, "summaryMetrics")
            .Where(item => (GetString(item, "label") ?? string.Empty).Contains("Onay", StringComparison.OrdinalIgnoreCase))
            .Sum(item => GetInt(item, "value"));

        if (consultants.Count == 0 && approvedDays == 0)
        {
            return RangeNoData(plan);
        }

        return $"{FormatRange(plan)} ekip izin ozeti: {onLeave} kisi en az bir gun izinli, toplam {approvedDays} onayli izin gunu gorunuyor.";
    }

    private static IReadOnlyList<JsonElement> GetArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return property.EnumerateArray().ToArray();
    }

    private static string? GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static int GetInt(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value)
            ? value
            : 0;

    private static DateOnly? GetDate(JsonElement element, string propertyName) =>
        DateOnly.TryParse(GetString(element, propertyName), out var date) ? date : null;

    private static int SumStatus(IReadOnlyList<JsonElement> statuses, string status) =>
        statuses.Where(item => IsStatus(item, status)).Sum(item => GetInt(item, "requestCount"));

    private static bool IsStatus(JsonElement item, string status) =>
        string.Equals(GetString(item, "status"), status, StringComparison.OrdinalIgnoreCase);

    private static string FormatRange(AiWorkforceQueryPlan plan) => plan.StartDate == plan.EndDate
        ? $"{plan.StartDate:yyyy-MM-dd} icin"
        : $"{plan.StartDate:yyyy-MM-dd} - {plan.EndDate:yyyy-MM-dd} araliginda";

    private static string RangeNoData(AiWorkforceQueryPlan plan) => $"{FormatRange(plan)} kayit bulunamadi.";
}
