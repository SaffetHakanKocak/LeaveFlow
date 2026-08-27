using System.ComponentModel.DataAnnotations;
using LeaveFlow.Application.People;

namespace LeaveFlow.Web.Models;

public sealed class ManagerAssignmentsViewModel
{
    [Required]
    public Guid ManagerId { get; init; }

    public string ManagerName { get; init; } = string.Empty;

    public IReadOnlyList<ManagerConsultantAssignment> Assignments { get; init; } = [];

    public IReadOnlyList<ConsultantListItem> AvailableConsultants { get; init; } = [];

    [Required]
    public Guid? ConsultantId { get; set; }
}
