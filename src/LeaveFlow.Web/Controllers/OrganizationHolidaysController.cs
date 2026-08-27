using LeaveFlow.Application.Abstractions.Holidays;
using LeaveFlow.Application.Authorization;
using LeaveFlow.Application.Holidays;
using LeaveFlow.Application.People;
using LeaveFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveFlow.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.RequireAdministrator)]
public sealed class OrganizationHolidaysController(
    IOrganizationHolidayService holidayService,
    IOrganizationHolidayRepository holidayRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        bool? isActive,
        DateOnly? fromDate,
        DateOnly? toDate,
        int pageNumber = 1,
        int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var results = await holidayService.SearchAsync(
            new HolidaySearchRequest(search, isActive, fromDate, toDate, pageNumber, pageSize),
            cancellationToken);

        return View(new OrganizationHolidayListViewModel
        {
            Search = search,
            IsActive = isActive,
            FromDate = fromDate,
            ToDate = toDate,
            Results = results
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var holiday = await holidayRepository.GetByIdAsync(id, cancellationToken);
        return holiday is null ? NotFound() : View(holiday);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new OrganizationHolidayFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrganizationHolidayFormViewModel model, CancellationToken cancellationToken)
    {
        var input = ToInput(model);
        if (!ModelState.IsValid || !AddValidationErrors(holidayService.Validate(input)))
        {
            return View(model);
        }

        var id = await holidayRepository.CreateAsync(input, cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var holiday = await holidayRepository.GetByIdAsync(id, cancellationToken);
        return holiday is null ? NotFound() : View(ToForm(holiday));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, OrganizationHolidayFormViewModel model, CancellationToken cancellationToken)
    {
        var input = ToInput(model);
        if (!ModelState.IsValid || !AddValidationErrors(holidayService.Validate(input)))
        {
            return View(model);
        }

        var updated = await holidayRepository.UpdateAsync(id, input, cancellationToken);
        return updated ? RedirectToAction(nameof(Details), new { id }) : NotFound();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        await holidayRepository.SetActiveAsync(id, isActive, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var holiday = await holidayRepository.GetByIdAsync(id, cancellationToken);
        return holiday is null ? NotFound() : View(holiday);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await holidayRepository.DeleteAsync(id, cancellationToken);
        return deleted ? RedirectToAction(nameof(Index)) : NotFound();
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

    private static OrganizationHolidayInput ToInput(OrganizationHolidayFormViewModel model)
    {
        return new OrganizationHolidayInput(model.Name, model.StartDate, model.EndDate, model.IsActive);
    }

    private static OrganizationHolidayFormViewModel ToForm(OrganizationHolidayDetail holiday)
    {
        return new OrganizationHolidayFormViewModel
        {
            Id = holiday.Id,
            Name = holiday.Name,
            StartDate = holiday.StartDate,
            EndDate = holiday.EndDate,
            IsActive = holiday.IsActive
        };
    }
}
