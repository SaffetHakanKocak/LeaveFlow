using LeaveFlow.Application.Abstractions.Ai;
using LeaveFlow.Application.Ai;
using LeaveFlow.Application.Identity;
using LeaveFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LeaveFlow.Web.Controllers;

[Authorize]
public sealed class AiAssistantController(
    IAiAssistantService assistantService,
    IOptions<AiOptions> aiOptions) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(CreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AiAssistantViewModel model, CancellationToken cancellationToken)
    {
        var settings = aiOptions.Value;
        if (!settings.Enabled)
        {
            return View(CreateViewModel(errorMessage: "AI Asistan şu anda kapalı."));
        }

        if (string.IsNullOrWhiteSpace(model.Prompt))
        {
            ModelState.AddModelError(nameof(model.Prompt), "Mesaj alanı zorunludur.");
        }
        else if (model.Prompt.Length > settings.MaxPromptLength)
        {
            ModelState.AddModelError(nameof(model.Prompt), $"Mesaj en fazla {settings.MaxPromptLength} karakter olabilir.");
        }

        if (!ModelState.IsValid)
        {
            return View(CreateViewModel(prompt: model.Prompt));
        }

        var result = await assistantService.SendAsync(
            new AiAssistantInput(
                User.GetRequiredUserId(),
                User.Identity?.Name ?? string.Empty,
                model.Prompt),
            cancellationToken);

        return View(CreateViewModel(
            prompt: model.Prompt,
            response: result.Message,
            errorMessage: result.ErrorMessage,
            isEnabled: result.IsEnabled,
            usedTools: result.UsedTools));
    }

    private AiAssistantViewModel CreateViewModel(
        string? prompt = null,
        string? response = null,
        string? errorMessage = null,
        bool? isEnabled = null,
        IReadOnlyList<string>? usedTools = null)
    {
        var settings = aiOptions.Value;
        return new AiAssistantViewModel
        {
            IsEnabled = isEnabled ?? settings.Enabled,
            MaxPromptLength = settings.MaxPromptLength,
            Prompt = prompt ?? string.Empty,
            Response = response,
            ErrorMessage = errorMessage,
            UsedTools = usedTools ?? []
        };
    }
}
