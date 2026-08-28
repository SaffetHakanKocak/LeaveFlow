using LeaveFlow.Application.Abstractions.Reporting;
using LeaveFlow.Application.Identity;
using LeaveFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveFlow.Web.Controllers;

[Authorize]
public sealed class DashboardController(IReportingService reportingService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var dashboard = await reportingService.GetDashboardAsync(User.GetRequiredUserId(), cancellationToken);
        return dashboard is null ? Forbid() : View(new DashboardViewModel { Dashboard = dashboard });
    }
}
