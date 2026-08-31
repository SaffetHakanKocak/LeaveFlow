using System.ComponentModel.DataAnnotations;

namespace LeaveFlow.Web.Models;

public sealed class AiAssistantViewModel
{
    public bool IsEnabled { get; init; }

    public int MaxPromptLength { get; init; }

    [Display(Name = "Mesaj")]
    [Required(ErrorMessage = "Mesaj alanı zorunludur.")]
    public string Prompt { get; set; } = string.Empty;

    public string? Response { get; init; }

    public string? ErrorMessage { get; init; }
}
