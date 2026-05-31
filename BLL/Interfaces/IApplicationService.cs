namespace BLL.Interfaces;

public interface IApplicationService
{
    
    Task<OperationResult<ApplicationDto>> ApplyAsync(
        ApplyDto dto, CancellationToken cancellationToken = default);

    Task<OperationResult<ApplicationDto>> ProposeAsync(
        ProposeDto dto, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VacancyDto>> GetLinkedVacanciesAsync(
        int resumeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResumeDto>> GetLinkedResumesAsync(
        int vacancyId, CancellationToken cancellationToken = default);
}
