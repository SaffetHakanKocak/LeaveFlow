using LeaveFlow.Api.Authentication;
using LeaveFlow.Api.ExceptionHandling;
using LeaveFlow.Application;
using LeaveFlow.Application.Authorization;
using LeaveFlow.Application.Identity;
using LeaveFlow.Infrastructure;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddLeaveFlowAuthorization();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = LeaveFlowAuthenticationSchemes.Api;
    options.DefaultChallengeScheme = LeaveFlowAuthenticationSchemes.Api;
})
.AddScheme<AuthenticationSchemeOptions, DeferredApiAuthenticationHandler>(
    LeaveFlowAuthenticationSchemes.Api,
    _ => { });

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
    await next(context);
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

if (app.Environment.IsEnvironment("Testing"))
{
    app.MapGet("/_test/throw", (HttpContext _) => throw new InvalidOperationException("Sensitive diagnostic exception marker."));
}

app.Run();

public partial class Program;
