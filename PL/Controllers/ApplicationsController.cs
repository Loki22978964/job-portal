using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PL.Controllers;

[Route("api/applications")]
[Authorize]
public sealed class ApplicationsController : ApiControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationsController(IApplicationService applicationService)
    {
        ArgumentNullException.ThrowIfNull(applicationService);
        _applicationService = applicationService;
    }

    [HttpPost("apply")]
    [Authorize(Roles = "Employee,Administrator")]
    public async Task<IActionResult> Apply(
        [FromBody] ApplyRequest request, CancellationToken cancellationToken)
    {
        var dto    = ApplicationMapper.ToApplyDto(request, GetCurrentUserId());
        var result = await _applicationService.ApplyAsync(dto, cancellationToken);
        return Ok(ApplicationMapper.ToViewModel(result.Data!));
    }

    [HttpGet("resume/{id:int}")]
    public async Task<IActionResult> GetLinkedVacancies(int id, CancellationToken cancellationToken)
    {
        var vacancies = await _applicationService.GetLinkedVacanciesAsync(id, cancellationToken);
        return Ok(vacancies.Select(VacancyMapper.ToViewModel));
    }

    [HttpGet("vacancy/{id:int}")]
    public async Task<IActionResult> GetLinkedResumes(int id, CancellationToken cancellationToken)
    {
        var resumes = await _applicationService.GetLinkedResumesAsync(id, cancellationToken);
        return Ok(resumes.Select(ResumeMapper.ToViewModel));
    }
}
