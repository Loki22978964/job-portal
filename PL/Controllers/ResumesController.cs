using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PL.Controllers;

[Route("api/resumes")]
[Authorize]
public sealed class ResumesController : ApiControllerBase
{
    private readonly IResumeService _resumeService;

    public ResumesController(IResumeService resumeService)
    {
        ArgumentNullException.ThrowIfNull(resumeService);
        _resumeService = resumeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] ResumeFilterRequest filter, CancellationToken cancellationToken)
    {
        var filterDto = ResumeMapper.ToFilterDto(filter);
        var resumes   = await _resumeService.GetAllAsync(filterDto, cancellationToken);
        return Ok(resumes.Select(ResumeMapper.ToViewModel));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var resume = await _resumeService.GetByIdAsync(id, cancellationToken);
        return Ok(ResumeMapper.ToViewModel(resume));
    }

    [HttpPost]
    [Authorize(Roles = "Employee,Administrator")]
    public async Task<IActionResult> Create(
        [FromBody] CreateResumeRequest request, CancellationToken cancellationToken)
    {
        var dto    = ResumeMapper.ToCreateDto(request, GetCurrentUserId());
        var result = await _resumeService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.Id },
            ResumeMapper.ToViewModel(result.Data));
    }

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Employee,Administrator")]
    public async Task<IActionResult> Patch(
        int id, [FromBody] PatchResumeRequest request, CancellationToken cancellationToken)
    {
        var dto    = ResumeMapper.ToPatchDto(request);
        var result = await _resumeService.PatchAsync(id, dto, GetCurrentUserId(), cancellationToken);
        return Ok(ResumeMapper.ToViewModel(result.Data!));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Employee,Administrator")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _resumeService.DeleteAsync(id, GetCurrentUserId(), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:int}/matching-vacancies")]
    public async Task<IActionResult> GetMatchingVacancies(int id, CancellationToken cancellationToken)
    {
        var vacancies = await _resumeService.FindMatchingVacanciesAsync(id, cancellationToken);
        return Ok(vacancies.Select(VacancyMapper.ToViewModel));
    }
}
