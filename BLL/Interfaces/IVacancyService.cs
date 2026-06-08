namespace BLL.Interfaces;

public interface IVacancyService
{
    
    Task<IReadOnlyList<VacancyDto>> GetAllAsync(
        VacancyFilterDto? filter = null, CancellationToken cancellationToken = default);

    Task<VacancyDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VacancyDto>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<OperationResult<VacancyDto>> CreateAsync(
        CreateVacancyDto dto, CancellationToken cancellationToken = default);

    Task<OperationResult<VacancyDto>> UpdateAsync(
        int id, UpdateVacancyDto dto, int userId, CancellationToken cancellationToken = default);

    Task<OperationResult<VacancyDto>> PatchAsync(
        int id, PatchVacancyDto dto, int userId, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ResumeDto>> FindMatchingResumesAsync(
        int vacancyId, CancellationToken cancellationToken = default);
}
