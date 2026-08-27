using LeaveFlow.Application.Abstractions.People;
using LeaveFlow.Application.Authorization;
using LeaveFlow.Application.Identity;
using LeaveFlow.Application.People;
using LeaveFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveFlow.Web.Controllers;

[Authorize]
public sealed class ConsultantsController(
    IConsultantManagementService consultantService,
    IConsultantManagementRepository consultantRepository) : Controller
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.RequireAdministrator)]
    public async Task<IActionResult> Index(
        string? search,
        bool? isActive,
        int pageNumber = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var results = await consultantService.SearchAsync(
            new PeopleSearchRequest(search, isActive, pageNumber, pageSize),
            cancellationToken);

        return View(new ConsultantListViewModel
        {
            Search = search,
            IsActive = isActive,
            Results = results
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var consultant = await consultantService.GetForActorAsync(
            User.GetRequiredUserId(),
            id,
            cancellationToken);

        return consultant is null ? Forbid() : View(consultant);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.RequireAdministrator)]
    public IActionResult Create()
    {
        return View(new ConsultantFormViewModel());
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequireAdministrator)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ConsultantFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || !AddValidationErrors(consultantService.Validate(ToInput(model))))
        {
            return View(model);
        }

        var id = await consultantRepository.CreateAsync(ToInput(model), cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.RequireAdministrator)]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var consultant = await consultantRepository.GetByIdAsync(id, cancellationToken);
        return consultant is null ? NotFound() : View(ToForm(consultant));
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequireAdministrator)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ConsultantFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || !AddValidationErrors(consultantService.Validate(ToInput(model))))
        {
            return View(model);
        }

        var updated = await consultantRepository.UpdateAsync(id, ToInput(model), cancellationToken);
        return updated ? RedirectToAction(nameof(Details), new { id }) : NotFound();
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.RequireAdministrator)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        await consultantRepository.SetActiveAsync(id, isActive, cancellationToken);
        return RedirectToAction(nameof(Index));
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

    private static ConsultantInput ToInput(ConsultantFormViewModel model)
    {
        return new ConsultantInput(
            model.FirstName,
            model.LastName,
            model.Email,
            model.EmployeeNumber,
            model.Department,
            model.StartDate,
            model.IsActive);
    }

    private static ConsultantFormViewModel ToForm(ConsultantDetail consultant)
    {
        return new ConsultantFormViewModel
        {
            Id = consultant.Id,
            FirstName = consultant.FirstName,
            LastName = consultant.LastName,
            Email = consultant.Email,
            EmployeeNumber = consultant.EmployeeNumber,
            Department = consultant.Department,
            StartDate = consultant.StartDate,
            IsActive = consultant.IsActive
        };
    }
}
