namespace DAL.Interfaces;

public interface IApplicationRepository : IRepository<Application>
{
    
    Task<IReadOnlyList<Application>> GetByResumeIdAsync(int resumeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Application>> GetByVacancyIdAsync(int vacancyId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int resumeId, int vacancyId, CancellationToken cancellationToken = default);
}
