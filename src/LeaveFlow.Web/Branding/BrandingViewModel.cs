using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace LeaveFlow.Web.Branding;

public sealed record BrandingViewModel(
    string OrganizationName,
    string ProductName,
    string ShortName,
    string? LogoUrl,
    string? PrimaryBrandColor,
    string? SupportEmail,
    string? FooterText)
{
    private const string DefaultOrganizationName = "Organization";
    private const string DefaultProductName = "LeaveFlow";
    private const string DefaultShortName = "LeaveFlow";

    private static readonly Regex HexColorPattern = new("^#[0-9a-fA-F]{6}$|^#[0-9a-fA-F]{3}$", RegexOptions.Compiled);
    private static readonly EmailAddressAttribute EmailValidator = new();

    public bool HasLogo => !string.IsNullOrWhiteSpace(LogoUrl);

    public bool HasCustomPrimaryBrandColor => !string.IsNullOrWhiteSpace(PrimaryBrandColor);

    public string BrandMarkText
    {
        get
        {
            var source = ShortName.Trim();
            if (source.Length <= 2)
            {
                return source.ToUpperInvariant();
            }

            var capitalLetters = new string(source.Where(char.IsUpper).Take(2).ToArray());
            if (capitalLetters.Length == 2)
            {
                return capitalLetters;
            }

            var initials = new string(source
                .Split([' ', '-', '_'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => part[0])
                .Take(2)
                .ToArray());

            return initials.Length == 2 ? initials.ToUpperInvariant() : source[..2].ToUpperInvariant();
        }
    }

    public static BrandingViewModel From(BrandingOptions? options)
    {
        options ??= new BrandingOptions();

        return new BrandingViewModel(
            RequiredText(options.OrganizationName, DefaultOrganizationName),
            RequiredText(options.ProductName, DefaultProductName),
            RequiredText(options.ShortName, DefaultShortName),
            SafeLogoUrl(options.LogoUrl),
            SafePrimaryBrandColor(options.PrimaryBrandColor),
            SafeEmail(options.SupportEmail),
            OptionalText(options.FooterText));
    }

    private static string RequiredText(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static string? OptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? SafePrimaryBrandColor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return HexColorPattern.IsMatch(trimmed) ? trimmed : null;
    }

    private static string? SafeLogoUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.StartsWith("/", StringComparison.Ordinal) &&
            !trimmed.StartsWith("//", StringComparison.Ordinal) &&
            !trimmed.Contains("\\", StringComparison.Ordinal) &&
            !trimmed.Contains("<", StringComparison.Ordinal) &&
            !trimmed.Contains(">", StringComparison.Ordinal) &&
            !trimmed.Contains("\"", StringComparison.Ordinal) &&
            !trimmed.Contains("'", StringComparison.Ordinal))
        {
            return trimmed;
        }

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
        {
            return null;
        }

        return uri.Scheme is "https" or "http" ? uri.ToString() : null;
    }

    private static string? SafeEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return EmailValidator.IsValid(trimmed) ? trimmed : null;
    }
}
