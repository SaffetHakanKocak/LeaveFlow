namespace LeaveFlow.Web.Branding;

public sealed class BrandingOptions
{
    public const string SectionName = "LeaveFlow:Branding";

    public string OrganizationName { get; set; } = "Organization";

    public string ProductName { get; set; } = "LeaveFlow";

    public string ShortName { get; set; } = "LeaveFlow";

    public string? LogoUrl { get; set; }

    public string? PrimaryBrandColor { get; set; }

    public string? SupportEmail { get; set; }

    public string? FooterText { get; set; }
}
