namespace BLL.Interfaces;

public interface IResumeService
{
    
    Task<IReadOnlyList<ResumeDto>> GetAllAsync(
        ResumeFilterDto? filter = null, CancellationToken cancellationToken = default);

    Task<ResumeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResumeDto>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<OperationResult<ResumeDto>> CreateAsync(
        CreateResumeDto dto, CancellationToken cancellationToken = default);

    Task<OperationResult<ResumeDto>> UpdateAsync(
        int id, UpdateResumeDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VacancyDto>> FindMatchingVacanciesAsync(
        int resumeId, CancellationToken cancellationToken = default);
}
