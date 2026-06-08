using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PL.Controllers;

[Route("api/vacancies")]
[Authorize]
public sealed class VacanciesController : ApiControllerBase
{
    private readonly IVacancyService _vacancyService;

    public VacanciesController(IVacancyService vacancyService)
    {
        ArgumentNullException.ThrowIfNull(vacancyService);
        _vacancyService = vacancyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] VacancyFilterRequest filter, CancellationToken cancellationToken)
    {
        var filterDto  = VacancyMapper.ToFilterDto(filter);
        var vacancies  = await _vacancyService.GetAllAsync(filterDto, cancellationToken);
        return Ok(vacancies.Select(VacancyMapper.ToViewModel));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var vacancy = await _vacancyService.GetByIdAsync(id, cancellationToken);
        return Ok(VacancyMapper.ToViewModel(vacancy));
    }

    [HttpPost]
    [Authorize(Roles = "Employer,Administrator")]
    public async Task<IActionResult> Create(
        [FromBody] CreateVacancyRequest request, CancellationToken cancellationToken)
    {
        var dto    = VacancyMapper.ToCreateDto(request, GetCurrentUserId());
        var result = await _vacancyService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            VacancyMapper.ToViewModel(result.Data));
    }

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Employer,Administrator")]
    public async Task<IActionResult> Patch(
        int id, [FromBody] PatchVacancyRequest request, CancellationToken cancellationToken)
    {
        var dto    = VacancyMapper.ToPatchDto(request);
        var result = await _vacancyService.PatchAsync(id, dto, GetCurrentUserId(), cancellationToken);
        return Ok(VacancyMapper.ToViewModel(result.Data!));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Employer,Administrator")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _vacancyService.DeleteAsync(id, GetCurrentUserId(), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:int}/matching-resumes")]
    public async Task<IActionResult> GetMatchingResumes(int id, CancellationToken cancellationToken)
    {
        var resumes = await _vacancyService.FindMatchingResumesAsync(id, cancellationToken);
        return Ok(resumes.Select(ResumeMapper.ToViewModel));
    }
}
