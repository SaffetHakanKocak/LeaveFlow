using System.Text.Json;

namespace LeaveFlow.Application.Ai;

internal static class AiToolArgumentReader
{
    internal static bool TryParse(string argumentsJson, out JsonElement root)
    {
        root = default;
        if (string.IsNullOrWhiteSpace(argumentsJson))
        {
            argumentsJson = "{}";
        }

        try
        {
            using var document = JsonDocument.Parse(argumentsJson);
            root = document.RootElement.Clone();
            return root.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    internal static DateOnly? GetDate(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String
            && DateOnly.TryParse(property.GetString(), out var value)
            ? value
            : null;
    }

    internal static bool TryGetDate(JsonElement root, string name, out DateOnly? value)
    {
        value = null;
        if (!root.TryGetProperty(name, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        if (property.ValueKind != JsonValueKind.String || !DateOnly.TryParse(property.GetString(), out var parsed))
        {
            return false;
        }

        value = parsed;
        return true;
    }

    internal static string? GetString(JsonElement root, string name, int maxLength)
    {
        if (!root.TryGetProperty(name, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var value = property.GetString();
        return string.IsNullOrWhiteSpace(value) || value.Length > maxLength ? null : value.Trim();
    }

    internal static Guid? GetGuid(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var property) || property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return property.ValueKind == JsonValueKind.String
            && Guid.TryParse(property.GetString(), out var value)
            ? value
            : null;
    }

    internal static bool IsValidRange(DateOnly? startDate, DateOnly? endDate, int maxDays)
    {
        if (startDate is null || endDate is null)
        {
            return true;
        }

        return startDate <= endDate && endDate.Value.DayNumber - startDate.Value.DayNumber + 1 <= maxDays;
    }
}
