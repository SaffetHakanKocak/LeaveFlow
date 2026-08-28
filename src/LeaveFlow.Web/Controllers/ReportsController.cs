using LeaveFlow.Application.Abstractions.Reporting;
using LeaveFlow.Application.Identity;
using LeaveFlow.Application.People;
using LeaveFlow.Application.Reporting;
using LeaveFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveFlow.Web.Controllers;

[Authorize]
public sealed class ReportsController(IReportingService reportingService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        DateOnly? startDate,
        DateOnly? endDate,
        Guid? managerId,
        Guid? consultantId,
        CancellationToken cancellationToken)
    {
        var query = new ReportQuery(startDate, endDate, managerId, consultantId);
        var validation = reportingService.Validate(query);
        if (!validation.IsValid)
        {
            AddValidationErrors(validation);
            return View(new ReportsViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                ManagerId = managerId,
                ConsultantId = consultantId
            });
        }

        var reports = await reportingService.GetReportsAsync(User.GetRequiredUserId(), query, cancellationToken);
        if (reports is null)
        {
            return Forbid();
        }

        return View(new ReportsViewModel
        {
            StartDate = reports.Query.StartDate,
            EndDate = reports.Query.EndDate,
            ManagerId = reports.Query.ManagerId,
            ConsultantId = reports.Query.ConsultantId,
            Reports = reports
        });
    }

    private void AddValidationErrors(ValidationResult validation)
    {
        foreach (var error in validation.Errors)
        {
            foreach (var message in error.Value)
            {
                ModelState.AddModelError(error.Key, message);
            }
        }
    }
}
