namespace BLL.Interfaces;

public interface IApplicationService
{
    
    Task<OperationResult<ApplicationDto>> ApplyAsync(
        ApplyDto dto, CancellationToken cancellationToken = default);

    Task<OperationResult<ApplicationDto>> ProposeAsync(
        ProposeDto dto, CancellationToken cancellationToken = default);

    Task<OperationResult<ApplicationDto>> AcceptAsync(
        int id, int userId, CancellationToken cancellationToken = default);

    Task<OperationResult<ApplicationDto>> RejectAsync(
        int id, int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VacancyDto>> GetLinkedVacanciesAsync(
        int resumeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResumeDto>> GetLinkedResumesAsync(
        int vacancyId, CancellationToken cancellationToken = default);
}
