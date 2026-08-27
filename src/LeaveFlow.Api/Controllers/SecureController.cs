using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/secure")]
public sealed class SecureController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "ok" });
    }
}
