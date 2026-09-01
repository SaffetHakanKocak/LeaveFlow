using LeaveFlow.Application;
using LeaveFlow.Application.Authorization;
using LeaveFlow.Application.Identity;
using LeaveFlow.Infrastructure;
using LeaveFlow.Web.Branding;
using LeaveFlow.Web.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddLeaveFlowAuthorization();
builder.Services.Configure<BrandingOptions>(builder.Configuration.GetSection(BrandingOptions.SectionName));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = LeaveFlowAuthenticationSchemes.WebCookie;
    options.DefaultChallengeScheme = LeaveFlowAuthenticationSchemes.WebCookie;
    options.DefaultSignInScheme = LeaveFlowAuthenticationSchemes.WebCookie;
    options.DefaultSignOutScheme = LeaveFlowAuthenticationSchemes.WebCookie;
})
.AddCookie(LeaveFlowAuthenticationSchemes.WebCookie, options =>
{
    var settings = builder.Configuration
        .GetSection(AuthenticationSettings.SectionName)
        .Get<AuthenticationSettings>() ?? new AuthenticationSettings();

    AuthenticationCookieConfiguration.Apply(
        options,
        settings,
        builder.Environment.IsProduction());
});

var isProduction = builder.Environment.IsProduction();

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = isProduction
        ? CookieSecurePolicy.Always
        : CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    SecurityHeaderPolicy.Apply(context, app.Environment.IsProduction());
    await next(context);
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
