using LeaveFlow.Application.Abstractions.People;
using LeaveFlow.Application.Authorization;
using LeaveFlow.Application.People;
using LeaveFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveFlow.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.RequireAdministrator)]
public sealed class ManagersController(
    IManagerManagementService managerService,
    IManagerManagementRepository managerRepository,
    IConsultantManagementRepository consultantRepository,
    IManagerConsultantAssignmentRepository assignmentRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        bool? isActive,
        int pageNumber = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var results = await managerService.SearchAsync(
            new PeopleSearchRequest(search, isActive, pageNumber, pageSize),
            cancellationToken);

        return View(new ManagerListViewModel
        {
            Search = search,
            IsActive = isActive,
            Results = results
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var manager = await managerRepository.GetByIdAsync(id, cancellationToken);
        return manager is null ? NotFound() : View(manager);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ManagerFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ManagerFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || !AddValidationErrors(managerService.Validate(ToInput(model))))
        {
            return View(model);
        }

        var id = await managerRepository.CreateAsync(ToInput(model), cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var manager = await managerRepository.GetByIdAsync(id, cancellationToken);
        return manager is null ? NotFound() : View(ToForm(manager));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ManagerFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || !AddValidationErrors(managerService.Validate(ToInput(model))))
        {
            return View(model);
        }

        var updated = await managerRepository.UpdateAsync(id, ToInput(model), cancellationToken);
        return updated ? RedirectToAction(nameof(Details), new { id }) : NotFound();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        await managerRepository.SetActiveAsync(id, isActive, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Assignments(Guid id, CancellationToken cancellationToken)
    {
        var model = await BuildAssignmentsModelAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(Guid managerId, Guid consultantId, CancellationToken cancellationToken)
    {
        var manager = await managerRepository.GetByIdAsync(managerId, cancellationToken);
        var consultant = await consultantRepository.GetByIdAsync(consultantId, cancellationToken);
        if (manager is null || consultant is null)
        {
            return NotFound();
        }

        if (!manager.IsActive || !consultant.IsActive)
        {
            return BadRequest();
        }

        var assignments = await assignmentRepository.GetByManagerIdAsync(managerId, cancellationToken);
        if (assignments.Any(assignment => assignment.ConsultantId == consultantId))
        {
            return RedirectToAction(nameof(Assignments), new { id = managerId });
        }

        await assignmentRepository.AssignAsync(managerId, consultantId, cancellationToken);
        return RedirectToAction(nameof(Assignments), new { id = managerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAssignment(Guid managerId, Guid consultantId, CancellationToken cancellationToken)
    {
        await assignmentRepository.RemoveAsync(managerId, consultantId, cancellationToken);
        return RedirectToAction(nameof(Assignments), new { id = managerId });
    }

    private async Task<ManagerAssignmentsViewModel?> BuildAssignmentsModelAsync(Guid managerId, CancellationToken cancellationToken)
    {
        var manager = await managerRepository.GetByIdAsync(managerId, cancellationToken);
        if (manager is null)
        {
            return null;
        }

        var assignments = await assignmentRepository.GetByManagerIdAsync(managerId, cancellationToken);
        var consultants = await consultantRepository.SearchAsync(
            new PeopleSearchRequest(null, true, 1, 100),
            cancellationToken);

        var assignedIds = assignments.Select(assignment => assignment.ConsultantId).ToHashSet();

        return new ManagerAssignmentsViewModel
        {
            ManagerId = manager.Id,
            ManagerName = $"{manager.FirstName} {manager.LastName}",
            Assignments = assignments,
            AvailableConsultants = consultants.Items
                .Where(consultant => !assignedIds.Contains(consultant.Id))
                .ToArray()
        };
    }

    private bool AddValidationErrors(ValidationResult validation)
    {
        foreach (var error in validation.Errors)
        {
            foreach (var message in error.Value)
            {
                ModelState.AddModelError(error.Key, message);
            }
        }

        return validation.IsValid;
    }

    private static ManagerInput ToInput(ManagerFormViewModel model)
    {
        return new ManagerInput(
            model.FirstName,
            model.LastName,
            model.Email,
            model.Department,
            model.IsActive);
    }

    private static ManagerFormViewModel ToForm(ManagerDetail manager)
    {
        return new ManagerFormViewModel
        {
            Id = manager.Id,
            FirstName = manager.FirstName,
            LastName = manager.LastName,
            Email = manager.Email,
            Department = manager.Department,
            IsActive = manager.IsActive
        };
    }
}
