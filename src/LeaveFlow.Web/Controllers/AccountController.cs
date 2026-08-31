using LeaveFlow.Application.Abstractions.Identity;
using LeaveFlow.Application.Identity;
using LeaveFlow.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveFlow.Web.Controllers;

public sealed class AccountController(ILoginService loginService) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(model.ReturnUrl);
        }

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, AuthenticationMessages.InvalidCredentials);
            model.Password = string.Empty;
            return View(model);
        }

        var result = await loginService.LoginAsync(
            new LoginRequest(
                model.Email,
                model.Password,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                HttpContext.TraceIdentifier),
            cancellationToken);

        model.Password = string.Empty;

        if (!result.Succeeded || result.User is null)
        {
            ModelState.AddModelError(string.Empty, AuthenticationMessages.InvalidCredentials);
            return View(model);
        }

        await HttpContext.SignOutAsync(LeaveFlowAuthenticationSchemes.WebCookie);

        var principal = LeaveFlowPrincipalFactory.Create(result.User);
        await HttpContext.SignInAsync(
            LeaveFlowAuthenticationSchemes.WebCookie,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true
            });

        return RedirectToLocal(model.ReturnUrl);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            await loginService.RecordLogoutAsync(
                User.GetRequiredUserId(),
                HttpContext.TraceIdentifier,
                cancellationToken);
        }

        await HttpContext.SignOutAsync(LeaveFlowAuthenticationSchemes.WebCookie);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
