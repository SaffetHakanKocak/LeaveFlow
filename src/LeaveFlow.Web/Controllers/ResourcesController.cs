using LeaveFlow.Application.Abstractions.Authorization;
using LeaveFlow.Application.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveFlow.Web.Controllers;

[Authorize]
public sealed class ResourcesController(IConsultantResourceAuthorizationService consultantAuthorization) : Controller
{
    [HttpGet("/resources/consultants/{id:guid}")]
    public async Task<IActionResult> Consultant(Guid id, CancellationToken cancellationToken)
    {
        var allowed = await consultantAuthorization.CanAccessConsultantAsync(
            User.GetRequiredUserId(),
            id,
            cancellationToken);

        if (!allowed)
        {
            return Forbid();
        }

        return Content("ok", "text/plain");
    }
}
