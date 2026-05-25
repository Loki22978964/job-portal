namespace DAL.Interfaces;

public interface IVacancyRepository : IRepository<Vacancy>
{
    
    Task<IReadOnlyList<Vacancy>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Vacancy>> GetAllWithUserAsync(CancellationToken cancellationToken = default);

    Task<Vacancy?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
